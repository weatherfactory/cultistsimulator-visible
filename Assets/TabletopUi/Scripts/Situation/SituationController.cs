using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.Core.Services;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Logic;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.TabletopUi {
    public class SituationController : ISituationSubscriber, ISaveable {

        public SituationToken situationToken;
        public SituationWindow situationWindow;
        public ISituationView situationWindowAsView;
        public ISituationStorage situationWindowAsStorage;
        public Situation Situation;

        private readonly ICompendium compendium;
        private readonly Character currentCharacter;

        private bool greedyAnimIsActive;
        private EndingFlavour _currentEndingFlavourToSignal=EndingFlavour.None; //encapsulating; want to be able to catch calls to thie slightly sloppy bit of state



        public bool IsOpen { get; set; }
		public Vector3 RestoreWindowPosition { get; set; }	// For saving window positions - CP

        public const float HOUSEKEEPING_CYCLE_BEATS = 1f;


        public bool IsOngoing {
            get { return Situation.State == SituationState.Ongoing; }
        }

        public EndingFlavour CurrentEndingFlavourToSignal
        {
            get { return _currentEndingFlavourToSignal; }
            set { _currentEndingFlavourToSignal = value; }
        }


        #region -- Construct, Initialize & Retire --------------------

        public SituationController(ICompendium co, Character ch) {
            compendium = co;
            currentCharacter = ch;
        }

        public void Initialise(Situation situation, SituationToken t, SituationWindow w) {
            Registry.Get<SituationsCatalogue>().RegisterSituation(this);
            Situation = situation;
            Situation.AddSubscriber(this);

            situationToken = t;
            


            if (situation.SourceToken != null)
				SoundManager.PlaySfx("SituationTokenCreate");

            situationWindow = w;
            situationWindowAsStorage = w;
            situationWindowAsView = w;

            

            switch (Situation.State) {
                case SituationState.Unstarted:
                    situationWindowAsStorage.SetUnstarted();
                    break;

                case SituationState.FreshlyStarted:
                case SituationState.Ongoing:
                case SituationState.RequiringExecution:
                    InitialiseForActiveSituation();
                    break;

                case SituationState.Complete:
                    InitialiseCompletedSituation();
                    break;

                default:
                    throw new ApplicationException("Tried to create situation for " + situation.Verb.Label + " with unknown state");
            }
        }

        void InitialiseForActiveSituation() {
            if (Situation.State == SituationState.FreshlyStarted)
                Situation.Start();

            //ugly subclause here. SituationClock.Start largely duplicates the constructor. I'm trying to use the same code path for recreating situations from a save file as for beginning a new situation
            //possibly just separating out FreshlyStarted would solve it

            situationWindowAsStorage.SetOngoing(Situation.currentPrimaryRecipe);

            situationToken.DisplayMiniSlot(Situation.currentPrimaryRecipe.Slots);
            situationToken.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining, CurrentEndingFlavourToSignal);
            situationWindowAsView.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining, CurrentEndingFlavourToSignal);

            //this is a little ugly here; but it makes the intent clear. The best way to deal with it is probably to pass the whole Command down to the situationwindow for processing.
            if (Situation.OverrideTitle != null)
                situationWindowAsView.Title = Situation.OverrideTitle;

            UpdateSituationDisplayForPossiblePredictedRecipe();

            if (Situation.currentPrimaryRecipe!=null && Situation.currentPrimaryRecipe.BurnImage != null)
                BurnImageUnderToken(Situation.currentPrimaryRecipe.BurnImage);

        }

        void InitialiseCompletedSituation() {
            situationWindowAsStorage.SetComplete();

            //this is a little ugly here; but it makes the intent clear. The best way to deal with it is probably to pass the whole Command down to the situationwindow for processing.
            if (Situation.OverrideTitle != null)
                situationWindowAsView.Title = Situation.OverrideTitle;

            //NOTE: only on Complete state. Completioncount shouldn't show on other states. This is fragile tho.
            if (Situation.CompletionCount >= 0)
                situationToken.SetCompletionCount(Situation.CompletionCount);
        }
        // Called from importer
        public void ModifyStoredElementStack(string elementId, int quantity, Context context)
        {
            situationWindowAsStorage.GetStorageStacksManager().ModifyElementQuantity(elementId, quantity, Source.Existing(), context);
            situationWindowAsView.DisplayStoredElements();
        }

        // Called from importer
        public void ReprovisionStoredElementStack(ElementStackSpecification stackSpecification, Source stackSource, string locatorid = null)
        {
            var stack=situationWindow.ReprovisionExistingElementStackInStorage(stackSpecification, stackSource, locatorid);
            situationWindowAsStorage.GetStorageStacksManager().AcceptStack(stack,new Context(Context.ActionSource.Loading));
            situationWindowAsView.DisplayStoredElements();
        }

        public void Retire() {
            situationWindowAsStorage.Retire();
            situationToken.Retire();
            Registry.Get<SituationsCatalogue>().DeregisterSituation(this);
        }

        #endregion


        public string GetTokenId() {
            return situationToken.EntityId;
        }

        public IAspectsDictionary GetAspectsAvailableToSituation(bool showElementAspects) {
            var aspects = situationWindowAsStorage.GetAspectsFromAllSlottedElements(showElementAspects);
            aspects.CombineAspects(situationWindowAsStorage.GetAspectsFromStoredElements(showElementAspects));
            return aspects;
        }

        // Compiles a dictionary of *every* aspect involved in the situation, so
        // that the extantreqs can be satisfied by elements that are in
        // completed situations
        public IAspectsDictionary GetAspectsInSituation()
        {
            var aspects = situationWindowAsStorage.GetAspectsFromAllSlottedAndStoredElements(true);
            aspects.CombineAspects(situationWindowAsStorage.GetAspectsFromOutputElements(true));
            return aspects;
        }

        private int GetNumOutputCards() {
            int count = 0;
            var stacks = situationWindowAsStorage.GetOutputStacks();

            foreach (var item in stacks) {
                if (item.Defunct)
                    continue;

                count += item.Quantity;
            }

            return count;
        }

        // Used to have greedy slots grab cards from within situations
        public IList<RecipeSlot> GetOngoingSlots() {
            return situationWindowAsStorage.GetOngoingSlots();
        }

        public IEnumerable<ElementStackToken> GetOutputStacks() {
            return situationWindowAsStorage.GetOutputStacks();
        }

        public IEnumerable<ElementStackToken> GetStartingStacks() {
            return situationWindowAsStorage.GetStartingStacks();
        }

        public IEnumerable<ElementStackToken> GetOngoingStacks()
        {
            return situationWindowAsStorage.GetOngoingStacks();
        }

        public IEnumerable<ElementStackToken> GetStoredStacks()
        {
            return situationWindowAsStorage.GetStoredStacks();
        }

        
        public HeartbeatResponse ExecuteHeartbeat(float interval) {
            HeartbeatResponse response = new HeartbeatResponse();
            var ttm = Registry.Get<TabletopManager>();
            var aspectsInContext = ttm.GetAspectsInContext(GetAspectsAvailableToSituation(true));

            RecipeConductor rc = new RecipeConductor(compendium,
               aspectsInContext, Registry.Get<IDice>(), currentCharacter);

            Situation.Continue(rc, interval, greedyAnimIsActive);

            // only pull in something if we've got a second remaining
            if (Situation.State == SituationState.Ongoing && Situation.TimeRemaining > HOUSEKEEPING_CYCLE_BEATS) {
                var tokenAndSlot = new TokenAndSlot() {
                    Token = situationToken as SituationToken,
                    RecipeSlot = situationWindowAsStorage.GetUnfilledGreedySlot() as RecipeSlot
                };

                if (tokenAndSlot.RecipeSlot != null && !tokenAndSlot.Token.Defunct && !tokenAndSlot.RecipeSlot.Defunct)
                {
                    response.SlotsToFill.Add(tokenAndSlot);
                }
            }

            return response;
        }

        public void SituationBeginning(Recipe withRecipe) {
            situationToken.DisplayStackInMiniSlot(null); // Hide content of miniSlotDisplay - looping recipes never go by complete which would do that
            situationToken.DisplayMiniSlot(withRecipe.Slots);
            situationWindowAsStorage.SetOngoing(withRecipe);
            StoreStacks(situationWindowAsStorage.GetStartingStacks());

            UpdateSituationDisplayForPossiblePredictedRecipe();

            situationWindowAsView.DisplayAspects(GetAspectsAvailableToSituation(false));

            if (withRecipe.BurnImage != null)
                BurnImageUnderToken(withRecipe.BurnImage);

        }

        private void PossiblySignalImpendingDoom(EndingFlavour endingFlavour)
        {
            var tabletopManager = Registry.Get<TabletopManager>();
            if (endingFlavour != EndingFlavour.None)
                tabletopManager.SignalImpendingDoom(situationToken);
            else
                tabletopManager.NoMoreImpendingDoom(situationToken);

        }

        public void StoreStacks(IEnumerable<ElementStackToken> inputStacks)
		{
          //  var inputStacks = situationWindow.GetOngoingStacks(); //This line looked like a mistake: the parameter for inputStacks was ignored (and it was named differently). Leaving in for now in case of sinister confusion - was there a reason we couldn't accept them?
            var storageStackManager = situationWindowAsStorage.GetStorageStacksManager();
            storageStackManager.AcceptStacks(inputStacks, new Context(Context.ActionSource.SituationStoreStacks));
            situationWindowAsView.DisplayStoredElements(); //displays the miniversion of the cards.
        }

        public void AddToResults(ElementStackToken stack, Context context)
		{
            situationWindowAsStorage.GetResultsStacksManager().AcceptStack(stack, context);
            UpdateTokenResultsCountBadge();

			var tabletop = Registry.Get<TabletopManager>() as TabletopManager;
			tabletop.NotifyAspectsDirty();	// Notify tabletop that aspects will need recompiling
        }

        public void SituationOngoing()
		{
            //var currentRecipe = compendium.GetRecipeById(SituationClock.RecipeId);
            situationToken.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining, CurrentEndingFlavourToSignal);
            situationWindowAsView.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining, CurrentEndingFlavourToSignal);
        }

        /// <summary>
        /// respond to the SituationClock's request to execute its payload
        /// </summary>
        /// <param name="command"></param>
        public void SituationExecutingRecipe(ISituationEffectCommand command) {
            var tabletopManager = Registry.Get<TabletopManager>();

            //called here in case ongoing slots trigger consumption
            situationWindowAsStorage.SetSlotConsumptions();

            //move any elements currently in OngoingSlots to situation storage
            //NB we're doing this *before* we execute the command - the command may affect these elements too
            StoreStacks(situationWindowAsStorage.GetOngoingStacks());



            if (command.AsNewSituation) {

                List<ElementStackToken> stacksToAddToNewSituation=new List<ElementStackToken>();
                //if there's an expulsion
                if (command.Expulsion.Limit>0)
                {
                    //find one or more matching stacks. Important! the limit applies to stacks, not cards. This might need to change.
                    AspectMatchFilter filter = new AspectMatchFilter(command.Expulsion.Filter);
                    var filteredStacks = filter.FilterElementStacks(situationWindowAsStorage.GetStoredStacks()).ToList();
                    if (filteredStacks.Any() && command.Expulsion.Limit > 0)
                    {
                        while (filteredStacks.Count > command.Expulsion.Limit)
                        {
                            filteredStacks.RemoveAt(filteredStacks.Count - 1);
                        }

                        stacksToAddToNewSituation = filteredStacks;
                    }


                    //nb if 2, there might be two stacks implicated

                    //take this opportunity to tidy stacks??
                }

                IVerb verbForNewSituation = compendium.GetEntityById<BasicVerb>(command.Recipe.ActionId);

                if (verbForNewSituation == null)
                    verbForNewSituation = new CreatedVerb(command.Recipe.ActionId, command.Recipe.Label, command.Recipe.Description);

         

                var scc = new SituationCreationCommand(verbForNewSituation, command.Recipe, SituationState.FreshlyStarted, situationToken as DraggableToken);
                tabletopManager.BeginNewSituation(scc,stacksToAddToNewSituation);
                situationWindowAsView.DisplayStoredElements();             //in case expulsions have removed anything
                return;
            }

            currentCharacter.AddExecutionsToHistory(command.Recipe.Id, 1);
            var executor = new SituationEffectExecutor(tabletopManager);
            executor.RunEffects(command, situationWindowAsStorage.GetStorageStacksManager(), currentCharacter, Registry.Get<IDice>());

            if (!string.IsNullOrEmpty(command.Recipe.Ending)) {
                var ending = compendium.GetEntityById<Ending>(command.Recipe.Ending);
                tabletopManager.EndGame(ending, this);
            }


            situationWindowAsView.DisplayStoredElements();

            TryOverrideVerbIcon(situationWindowAsStorage.GetAspectsFromStoredElements(true)); //this is called from OngoingSlotsOrStorageUpdated too. But I'm not going to call that here
            //cos I can't remember the details of the flow and I don't want to end up in a loop somewhere


        }

        private void TryOverrideVerbIcon(IAspectsDictionary forAspects)
        {
//if we have an element in the situation now that overrides the verb icon, update it
            string overrideIcon = compendium.GetVerbIconOverrideFromAspects(forAspects);
            if (!string.IsNullOrEmpty(overrideIcon))
            { 
                situationToken.DisplayOverrideIcon(overrideIcon);
                situationWindowAsView.DisplayIcon(overrideIcon);
            }
        }

        public void ReceiveAndRefineTextNotification(INotification notification)
        {
            //Check for possible text refinements based on the aspects in context
            var aspectsInSituation = GetAspectsAvailableToSituation(true);
            var outputAspects = situationWindowAsStorage.GetAspectsFromOutputElements(true);
            aspectsInSituation.CombineAspects(outputAspects);


            TextRefiner tr=new TextRefiner(aspectsInSituation);


            Notification refinedNotification=new Notification(notification.Title,
                tr.RefineString(notification.Description));


            situationWindowAsStorage.ReceiveTextNote(refinedNotification);
        }


        /// <summary>
        /// The situation is complete. DisplayHere the output cards and description
        /// </summary>
        public void SituationComplete() {
            var outputStacks = situationWindowAsStorage.GetStoredStacks();
            INotification notification = new Notification(Situation.GetTitle(), Situation.GetDescription());
            SetOutput(outputStacks.ToList());

            ReceiveAndRefineTextNotification(notification);


            //This must be run here: it disables (and destroys) any card tokens that have not been moved to outputs
            situationWindowAsStorage.SetComplete();

            // Now update the token based on the current stacks in the window
            situationToken.DisplayComplete();
            situationToken.SetCompletionCount(GetNumOutputCards());
            situationToken.DisplayStackInMiniSlot(situationWindowAsStorage.GetOngoingStacks());

            AttemptAspectInductions();

            var currentRecipe = compendium.GetEntityById<Recipe>(Situation.RecipeId);

            if (currentRecipe.PortalEffect != PortalEffect.None)
            {
                // Add a check to see if this is actually running in the game
                var behaviour = situationToken as MonoBehaviour;
                if (behaviour != null)
                    Registry.Get<TabletopManager>().ShowMansusMap(this, behaviour.transform,
                        currentRecipe.PortalEffect);
            }
        }

        public void Halt() {
            //currently used only in debug. Reset to starting state (which might be weird for Time) and end timer.
            Situation.Halt();
            //If we leave anything in the ongoing slot, it's lost, and also the situation ends up in an anomalous state which breaks loads
            situationWindowAsStorage.SetOutput(situationWindowAsStorage.GetOngoingStacks().ToList());
        }

        private void AttemptAspectInductions() {
            //If any elements in the output, or in the situation itself, have inductions, test whether to start a new recipe

            var outputStacks = situationWindowAsStorage.GetOutputStacks();
            var inducingAspects=new AspectsDictionary();

            //face-down cards don't trigger inductions. This is because we don't generally want to trigger an induction
            //for something that has JUST BEEN CREATED. This is unmistakably a hack, and if we distinguish newly added cards better
            //(or expand IsFront to cover that) then that would be safer.

            //It will may mean that we can't transform a card that we don't want to trigger initial inductions and may have to delete/recreate it, losing mutations.

            foreach (var os in outputStacks)
            {
               if(os.IsFront())
                   inducingAspects.CombineAspects(os.GetAspects());
            }

            var currentRecipe = compendium.GetEntityById<Recipe>(Situation.RecipeId);

            inducingAspects.CombineAspects(currentRecipe.Aspects);


            foreach (var a in inducingAspects) {
                var aspectElement = compendium.GetEntityById<Element>(a.Key);

                if (aspectElement != null)
                    PerformAspectInduction(aspectElement);
                else
                    NoonUtility.Log("unknown aspect " + a + " in output");
            }
        }

        void PerformAspectInduction(Element aspectElement) {
            foreach (var induction in aspectElement.Induces) {
                var d = Registry.Get<IDice>();

                if (d.Rolld100() <= induction.Chance)
                    CreateRecipeFromInduction(compendium.GetEntityById<Recipe>(induction.Id), aspectElement.Id);
            }
        }

        void CreateRecipeFromInduction(Recipe inducedRecipe, string aspectID) {
            if (inducedRecipe == null) {
                NoonUtility.Log("unknown recipe " + inducedRecipe + " in induction for " + aspectID);
                return;
            }

            var inductionRecipeVerb = new CreatedVerb(inducedRecipe.ActionId,
                inducedRecipe.Label, inducedRecipe.Description);
            SituationCreationCommand inducedSituation = new SituationCreationCommand(inductionRecipeVerb,
                inducedRecipe, SituationState.FreshlyStarted, situationToken as DraggableToken);
            Registry.Get<TabletopManager>().BeginNewSituation(inducedSituation,new List<ElementStackToken>());
        }

        public void ResetSituation() {
            Situation.ResetIfComplete();

            //if this was a transient verb, clean up everything and finish.
            if (situationToken.IsTransient) {
                Retire();
            }
            else {
                situationWindowAsStorage.SetUnstarted();
                situationToken.SetCompletionCount(-1);
            }
        }

        public void OpenWindow( Vector3 targetPosOverride )
		{
            IsOpen = true;
            situationToken.DisplayAsOpen();
            situationWindowAsView.Show( targetPosOverride );
            Registry.Get<TabletopManager>().CloseAllSituationWindowsExcept(situationToken.EntityId);
        }

		public void OpenWindow()
		{
            OpenWindow( Vector3.zero );
        }

        public void CloseWindow() {
            IsOpen = false;
            // This comes first so the token doesn't show a glow when it's being closed
            situationWindowAsStorage.DumpAllStartingCardsToDesktop(); // only dumps if it can, obv.
            situationWindowAsView.Hide();

            situationToken.DisplayAsClosed();
        }

        public void ShowVisualEffectIfCanTakeDroppedToken(ElementStackToken stack,bool show)
        {
            situationToken.SetGlowColor(UIStyle.TokenGlowColor.Default);
            situationToken.ShowGlow(show && CanAcceptStackWhenClosed(stack));
        }

        public bool CanAcceptStackWhenClosed(ElementStackToken stack) {
            if (Situation.State == SituationState.Unstarted)
                return HasSuitableStartingSlot(stack);
            if (Situation.State == SituationState.Ongoing)
                return HasEmptyOngoingSlot(stack);

            return false;
        }

        bool HasSuitableStartingSlot(ElementStackToken forStack) {
            var win = situationWindow as SituationWindow;
            var primarySlot = win.GetPrimarySlot();

            if (primarySlot==null)
                return false;

            bool allowedInSlot = primarySlot.GetSlotMatchForStack(forStack).MatchType == SlotMatchForAspectsType.Okay;
            return allowedInSlot;


        }

        bool HasEmptyOngoingSlot(ElementStackToken stack) {
            var ongoingSlots = situationWindowAsStorage.GetOngoingSlots();

            if (ongoingSlots.Count == 0)
                return false;

            // Alexis want's token-drop actions to be able to replace existing tokens
            //if (ongoingSlots[0].GetElementStackInSlot() != null)
            //    return false;

            return ongoingSlots[0].IsGreedy == false && ongoingSlots[0].GetSlotMatchForStack(stack).MatchType == SlotMatchForAspectsType.Okay;
        }

        public bool PushDraggedStackIntoToken(ElementStackToken stack) {
            if (Situation.State == SituationState.Unstarted)
                return PushDraggedStackIntoStartingSlots(stack);
            if (Situation.State == SituationState.Ongoing)
                return PushDraggedStackIntoOngoingSlot(stack);



            return false;
        }

        bool PushDraggedStackIntoStartingSlots(ElementStackToken stack) {
            if (HasSuitableStartingSlot(stack) == false)
                return false;

            var win = situationWindow as SituationWindow;
            var primarySlot = win.GetPrimarySlot();

            return PushDraggedStackIntoSlot(stack, primarySlot);
        }

        bool PushDraggedStackIntoOngoingSlot(ElementStackToken stack) {
            if (HasEmptyOngoingSlot(stack) == false)
                return false;

            var ongoingSlots = situationWindowAsStorage.GetOngoingSlots();

            return PushDraggedStackIntoSlot(stack, ongoingSlots[0]);
        }

        bool PushDraggedStackIntoSlot(ElementStackToken stack, RecipeSlot slot) {
            if (stack == null || slot == null)
                return false;

            var targetElement = slot.GetElementStackInSlot() as ElementStackToken;

            if (targetElement != null)
                targetElement.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));

            // On drop pushes the currently dragged stack into this slot, with all the movement and parenting
            slot.OnDrop(null);

            return true;
        }

        public void StartingSlotsUpdated() {
            // it's possible the starting slots have been updated because we've just started a recipe and stored everything that was in them.
            // in this case, don't do anything.
            if (Situation.State != SituationState.Unstarted)
                return;

            // Get all aspects and find a recipe
            IAspectsDictionary allAspectsInSituation = situationWindowAsStorage.GetAspectsFromAllSlottedElements(true);
            var tabletopManager = Registry.Get<TabletopManager>();
            var aspectsInContext = tabletopManager.GetAspectsInContext(allAspectsInSituation);
        
            Recipe matchingRecipe = compendium.GetFirstMatchingRecipe(aspectsInContext,  situationToken.EntityId, currentCharacter, false);

            // Update the aspects in the window
            IAspectsDictionary aspectsNoElementsSelf = situationWindowAsStorage.GetAspectsFromAllSlottedElements(false);
            situationWindowAsView.DisplayAspects(aspectsNoElementsSelf);

            //if we found a recipe, display it, and get ready to activate
            if (matchingRecipe != null) {


                situationWindowAsView.DisplayStartingRecipeFound(matchingRecipe);
                return;
            }

            //if we can't find a matching craftable recipe, check for matching hint recipes
            Recipe matchingHintRecipe = compendium.GetFirstMatchingRecipe(aspectsInContext, situationToken.EntityId, currentCharacter, true); ;

            //perhaps we didn't find an executable recipe, but we did find a hint recipe to display
            if (matchingHintRecipe != null)
                situationWindowAsView.DisplayHintRecipeFound(matchingHintRecipe);
            //no recipe, no hint? If there are any elements in the mix, display 'try again' message
            else if (allAspectsInSituation.Count > 0)
                situationWindowAsView.DisplayNoRecipeFound();
            //no recipe, no hint, no aspects. Just set back to unstarted
            else
                situationWindowAsStorage.SetUnstarted();
        }

        public void OngoingSlotsOrStorageUpdated() {
            //we don't display the elements themselves, just their aspects.
            //But we *do* take the elements themselves into consideration for determining recipe execution
            var aspectsToDisplayInBottomBar = GetAspectsAvailableToSituation(false);
            var allAspects = GetAspectsAvailableToSituation(true);

            situationWindowAsView.DisplayAspects(aspectsToDisplayInBottomBar);
            TabletopManager ttm = Registry.Get<TabletopManager>();

            var context = ttm.GetAspectsInContext(allAspects);

            if(Situation.currentPrimaryRecipe!=null)
            {

                var rp = GetNextRecipePrediction(context);

                if (rp != null)
                {
                    CurrentEndingFlavourToSignal = rp.SignalEndingFlavour;
                    if ( rp.BurnImage != null)
                        BurnImageUnderToken(rp.BurnImage);
                    PossiblySignalImpendingDoom(rp.SignalEndingFlavour);
                    //Check for possible text refinements based on the aspects in context
                    var aspectsInSituation = GetAspectsAvailableToSituation(true);
                    TextRefiner tr=new TextRefiner(aspectsInSituation);
                    rp.DescriptiveText = tr.RefineString(rp.DescriptiveText);
                    situationWindowAsView.UpdateTextForPrediction(rp);
                }

            }

            situationToken.DisplayStackInMiniSlot(situationWindowAsStorage.GetOngoingStacks());

            TryOverrideVerbIcon(situationWindowAsStorage.GetAspectsFromStoredElements(true));
        }

        private RecipePrediction GetNextRecipePrediction(AspectsInContext aspectsInContext) {
            RecipeConductor rc = new RecipeConductor(compendium, aspectsInContext,
                new DefaultDice(), currentCharacter); //nb the use of default dice: we don't want to display any recipes without a 100% chance of executing
            return Situation.GetPrediction(rc);
        }

        public void UpdateSituationDisplayForPossiblePredictedRecipe()
        {
            TabletopManager ttm = Registry.Get<TabletopManager>();
            var context = ttm.GetAspectsInContext(situationWindowAsStorage.GetAspectsFromAllSlottedAndStoredElements(true));

            RecipeConductor rc = new RecipeConductor(compendium, context, Registry.Get<IDice>(), currentCharacter);

            var nextRecipePrediction = Situation.GetPrediction(rc);
            //Check for possible text refinements based on the aspects in context
            var aspectsInSituation = GetAspectsAvailableToSituation(true);
            TextRefiner tr = new TextRefiner(aspectsInSituation);
            nextRecipePrediction.DescriptiveText = tr.RefineString(nextRecipePrediction.DescriptiveText);
            situationWindowAsView.UpdateTextForPrediction(nextRecipePrediction);
            CurrentEndingFlavourToSignal = nextRecipePrediction.SignalEndingFlavour;
            PossiblySignalImpendingDoom(nextRecipePrediction.SignalEndingFlavour);

        }

        public void SetOutput(List<ElementStackToken> stacksForOutput) {
            situationWindowAsStorage.SetOutput(stacksForOutput);
        }

        public void AddNote(INotification notification) {
            situationWindowAsStorage.ReceiveTextNote(notification);
        }

        public void ShowDestinationsForStack(ElementStackToken stack, bool show) {
            situationWindow.ShowDestinationsForStack(stack, show);
        }

        //also called from hotkey
        public void DumpAllResults() {
            if (Situation.State == SituationState.Complete)
                situationWindowAsStorage.DumpAllResultingCardsToDesktop();
        }

        /// <summary>
        /// if the situation is complete, decay any cards in results.
        /// Don't do anything with cards in slots for non-complete situations..... yet.
        /// </summary>
        public void TryDecayContents(float interval)
        {
            if (Situation.State == SituationState.Complete)
                situationWindowAsStorage.TryDecayResults(interval);
        }

        // letting other things change the situation

        public void AttemptActivateRecipe()
        {


            if (Situation.State != SituationState.Unstarted)
                return;

            var aspects = situationWindowAsStorage.GetAspectsFromAllSlottedElements(true);
            var tabletopManager = Registry.Get<TabletopManager>();
            var aspectsInContext = tabletopManager.GetAspectsInContext(aspects);


            var recipe = compendium.GetFirstMatchingRecipe(aspectsInContext, situationToken.EntityId, currentCharacter, false);

            //no recipe found? get outta here
            if (recipe == null)
                return;

            //kick off the situation. We want to do this first, so that modifying the stacks next won't cause the window to react
            //as if we're removing items from an unstarted situation
			Situation.StartRecipe(recipe);

			// Play the SFX here (not in the clock) so it is only played when we manually start
			SoundManager.PlaySfx("SituationBegin");

            //called here in case starting slots trigger consumption
            situationWindowAsStorage.SetSlotConsumptions();
            //move any slotted elements to storage
            situationWindowAsStorage.StoreStacks(situationWindowAsStorage.GetStartingStacks());

            //The game might be paused! or the player might just be incredibly quick off the mark
            //so immediately continue with a 0 interval - this won't advance time, but will update the visuals in the situation window
            //(which among other things should make the starting slot unavailable

            RecipeConductor rc = new RecipeConductor(compendium,
                aspectsInContext, Registry.Get<IDice>(), currentCharacter); //reusing the aspectsInContext from above

            Situation.Continue(rc, 0);

            //display any burn image the recipe might require

            if (recipe.BurnImage != null)
                BurnImageUnderToken(recipe.BurnImage);
        }

        public void NotifyGreedySlotAnim(TokenAnimationToSlot slotAnim) {
            greedyAnimIsActive = true;
            slotAnim.onElementSlotAnimDone += HandleOnGreedySlotAnimDone;

			TabletopManager.RequestNonSaveableState( TabletopManager.NonSaveableType.Greedy, true );

			// Hack to try to repro bug #1253 - CP
			//var tabletop = Registry.Retrieve<TabletopManager>();
			//tabletop.ForceAutosave();
        }

        void HandleOnGreedySlotAnimDone(ElementStackToken element, TokenAndSlot tokenSlotPair) {
            greedyAnimIsActive = false;
			TabletopManager.RequestNonSaveableState( TabletopManager.NonSaveableType.Greedy, false );
        }

        // Update Visuals

        public void UpdateTokenResultsCountBadge() {
            situationToken.SetCompletionCount(GetNumOutputCards());
        }

        private void BurnImageUnderToken(string burnImage) {
            Registry.Get<INotifier>()
                .ShowImageBurn(burnImage, situationToken as DraggableToken, 20f, 2f,
                    TabletopImageBurner.ImageLayoutConfig.CenterOnToken);
        }


        public IRecipeSlot GetSlotBySaveLocationInfoPath(string locationInfo, string slotType) {
            if (slotType == SaveConstants.SAVE_STARTINGSLOTELEMENTS)
                return situationWindowAsStorage.GetStartingSlotBySaveLocationInfoPath(locationInfo);
            else
                return situationWindow.GetOngoingSlotBySaveLocationInfoPath(locationInfo);
        }

        public Hashtable GetSaveData() {
            var situationSaveData = new Hashtable();
            IGameDataExporter exporter = new GameDataExporter();

			// Added by CP to allow autosave while player is working
			Vector3 pos = situationWindow.Position;
            situationSaveData.Add(SaveConstants.SAVE_SITUATION_WINDOW_OPEN, this.IsOpen);
			situationSaveData.Add(SaveConstants.SAVE_SITUATION_WINDOW_X, pos.x);
			situationSaveData.Add(SaveConstants.SAVE_SITUATION_WINDOW_Y, pos.y);
			situationSaveData.Add(SaveConstants.SAVE_SITUATION_WINDOW_Z, pos.z);

            situationSaveData.Add(SaveConstants.SAVE_VERBID, situationToken.EntityId);
            if (Situation != null) {
                situationSaveData.Add(SaveConstants.SAVE_TITLE, situationWindowAsView.Title);
                situationSaveData.Add(SaveConstants.SAVE_RECIPEID, Situation.RecipeId);
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONSTATE, Situation.State);
                situationSaveData.Add(SaveConstants.SAVE_TIMEREMAINING, Situation.TimeRemaining);

                situationSaveData.Add(SaveConstants.SAVE_COMPLETIONCOUNT, GetNumOutputCards());
            }

            //save stacks in window (starting) slots
            if (situationWindowAsStorage.GetStartingStacks().Any()) {
                var htStartingSlots = exporter.GetHashTableForStacks(situationWindowAsStorage.GetStartingStacks());
                situationSaveData.Add(SaveConstants.SAVE_STARTINGSLOTELEMENTS, htStartingSlots);
            }

            //save stacks in ongoing slots
            if (situationWindowAsStorage.GetOngoingStacks().Any()) {
                var htOngoingSlots = exporter.GetHashTableForStacks(situationWindowAsStorage.GetOngoingStacks());
                situationSaveData.Add(SaveConstants.SAVE_ONGOINGSLOTELEMENTS, htOngoingSlots);
            }

            //save stacks in storage
            if (situationWindowAsStorage.GetStoredStacks().Any()) {
                var htStacksInStorage = exporter.GetHashTableForStacks(situationWindowAsStorage.GetStoredStacks());
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONSTOREDELEMENTS, htStacksInStorage);
            }

            //save stacks in output
            if (situationWindowAsStorage.GetOutputStacks().Any()) {
                var htStacksInOutput = exporter.GetHashTableForStacks(situationWindowAsStorage.GetOutputStacks());
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS, htStacksInOutput);
            }

            //save notes, and their contents
            if (situationWindowAsStorage.GetNotes().Any()) {
                var htNotes = exporter.GetHashTableForSituationNotes(situationWindowAsStorage.GetNotes());
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONNOTES, htNotes);
            }
            return situationSaveData;
        }


        public void TryResizeWindow(int slotsCount)
        {
                situationWindow.SetWindowSize(slotsCount > 3);
        }
    }
}

