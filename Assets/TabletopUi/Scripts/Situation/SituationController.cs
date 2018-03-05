using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
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

        public ISituationAnchor situationToken;
        private ISituationDetails situationWindow;
        public ISituationClock SituationClock;

        private readonly ICompendium compendium;
        private readonly Character currentCharacter;

        private bool greedyAnimIsActive;

        public bool IsOpen { get; set; }

        public const float HOUSEKEEPING_CYCLE_BEATS = 1f;

        public bool EditorIsActive
        {
            get { return situationToken.EditorIsActive; }
        }

        public bool IsOngoing {
            get { return SituationClock.State == SituationState.Ongoing; }
        }

        #region -- Construct, Initialize & Retire --------------------

        public SituationController(ICompendium co, Character ch) {
            compendium = co;
            currentCharacter = ch;
        }

        public void Initialise(SituationCreationCommand command, ISituationAnchor t, ISituationDetails w) {
            Registry.Retrieve<SituationsCatalogue>().RegisterSituation(this);

            situationToken = t;
            situationToken.Initialise(command.GetBasicOrCreatedVerb(), this);

            situationWindow = w;
            situationWindow.Initialise(command.GetBasicOrCreatedVerb(), this);

            SituationClock = new SituationClock(command.TimeRemaining, command.State, command.Recipe, this);

            switch (command.State) {
                case SituationState.Unstarted:
                    situationWindow.SetUnstarted();
                    break;

                case SituationState.FreshlyStarted:
                case SituationState.Ongoing:
                case SituationState.RequiringExecution:
                    InitialiseActiveSituation(command);
                    break;

                case SituationState.Complete:
                    InitialiseCompletedSituation(command);
                    break;

                default:
                    throw new ApplicationException("Tried to create situation for " + command.Verb.Label + " with unknown state");
            }
        }

        void InitialiseActiveSituation(SituationCreationCommand command) {
            if (command.State == SituationState.FreshlyStarted)
                SituationClock.Start(command.Recipe);

            //ugly subclause here. SituationClock.Start largely duplicates the constructor. I'm trying to use the same code path for recreating situations from a save file as for beginning a new situation
            //possibly just separating out FreshlyStarted would solve it

            situationWindow.SetOngoing(command.Recipe);

            situationToken.DisplayMiniSlot(command.Recipe.SlotSpecifications);
            situationToken.DisplayTimeRemaining(SituationClock.Warmup, SituationClock.TimeRemaining, command.Recipe);
            situationWindow.DisplayTimeRemaining(SituationClock.Warmup, SituationClock.TimeRemaining, command.Recipe);

            //this is a little ugly here; but it makes the intent clear. The best way to deal with it is probably to pass the whole Command down to the situationwindow for processing.
            if (command.OverrideTitle != null)
                situationWindow.Title = command.OverrideTitle;
        }

        void InitialiseCompletedSituation(SituationCreationCommand command) {
            SituationClock = new SituationClock(this);
            SituationClock.State = SituationState.Complete;
            situationWindow.SetComplete();

            //this is a little ugly here; but it makes the intent clear. The best way to deal with it is probably to pass the whole Command down to the situationwindow for processing.
            if (command.OverrideTitle != null)
                situationWindow.Title = command.OverrideTitle;

            //NOTE: only on Complete state. Completioncount shouldn't show on other states. This is fragile tho.
            if (command.CompletionCount >= 0)
                situationToken.SetCompletionCount(command.CompletionCount);
        }

        // Called from importer
        public void ModifyStoredElementStack(string elementId, int quantity, Context context) {
            situationWindow.GetStorageStacksManager().ModifyElementQuantity(elementId, quantity, Source.Existing(), context);
            situationWindow.DisplayStoredElements();
        }

        public void Retire() {
            situationToken.Retire();
            situationWindow.Retire();
            Registry.Retrieve<SituationsCatalogue>().DeregisterSituation(this);
        }

        #endregion

        #region -- State Getters --------------------

        public string GetTokenId() {
            return situationToken.Id;
        }

        private IAspectsDictionary GetAspectsAvailableToSituation(bool showElementAspects) {
            var aspects = situationWindow.GetAspectsFromAllSlottedElements(showElementAspects);
            aspects.CombineAspects(situationWindow.GetAspectsFromStoredElements(showElementAspects));
            return aspects;
        }

        private int GetNumOutputCards() {
            int count = 0;
            var stacks = situationWindow.GetOutputStacks();

            foreach (var item in stacks) {
                if (item.Defunct)
                    continue;

                count += item.Quantity;
            }

            return count;
        }

        // Used to have greedy slots grab cards from within situations
        public IList<RecipeSlot> GetOngoingSlots() {
            return situationWindow.GetOngoingSlots();
        }

        public IEnumerable<IElementStack> GetOutputStacks() {
            return situationWindow.GetOutputStacks();
        }

        public IEnumerable<IElementStack> GetStartingStacks() {
            return situationWindow.GetStartingStacks();
        }

        #endregion

        #region -- SituationClock Execution (Heartbeat) --------------------

        public HeartbeatResponse ExecuteHeartbeat(float interval) {
            HeartbeatResponse response = new HeartbeatResponse();
            RecipeConductor rc = new RecipeConductor(compendium,
                GetAspectsAvailableToSituation(true), Registry.Retrieve<IDice>(), currentCharacter);
                        
            SituationClock.Continue(rc, interval, greedyAnimIsActive);

            // only pull in something if we've got a second remaining
            if (SituationClock.State == SituationState.Ongoing && SituationClock.TimeRemaining > HOUSEKEEPING_CYCLE_BEATS) {
                var tokenAndSlot = new TokenAndSlot() {
                    Token = situationToken as SituationToken,
                    RecipeSlot = situationWindow.GetUnfilledGreedySlot() as RecipeSlot
                };

                if (tokenAndSlot.RecipeSlot != null && !tokenAndSlot.Token.Defunct && !tokenAndSlot.RecipeSlot.Defunct)
                    response.SlotsToFill.Add(tokenAndSlot);
            }

            return response;
        }

        public void SituationBeginning(Recipe withRecipe) {
            situationToken.DisplayStackInMiniSlot(null); // Hide content of miniSlotDisplay - looping recipes never go by complete which would do that
            situationToken.DisplayMiniSlot(withRecipe.SlotSpecifications);
            situationWindow.SetOngoing(withRecipe);
            StoreStacks(situationWindow.GetStartingStacks());

            UpdateSituationDisplayForDescription();

            if (withRecipe.EndsGame()) {
                var tabletopManager = Registry.Retrieve<TabletopManager>();
                tabletopManager.SignalImpendingDoom(situationToken);
            }
        }

        void StoreStacks(IEnumerable<IElementStack> stacks) {
            var inputStacks = situationWindow.GetOngoingStacks();
            var storageStackManager = situationWindow.GetStorageStacksManager();
            storageStackManager.AcceptStacks(inputStacks, new Context(Context.ActionSource.SituationStoreStacks));
            situationWindow.DisplayStoredElements(); //displays the miniversion of the cards. This should 
        }

        public void AddToResults(ElementStackToken stack, Context context) {
            situationWindow.GetResultsStacksManager().AcceptStack(stack, context);
            UpdateTokenResultsCountBadge();
        }

        public void SituationOngoing() {
            var currentRecipe = compendium.GetRecipeById(SituationClock.RecipeId);
            situationToken.DisplayTimeRemaining(SituationClock.Warmup, SituationClock.TimeRemaining, currentRecipe);
            situationWindow.DisplayTimeRemaining(SituationClock.Warmup, SituationClock.TimeRemaining, currentRecipe);
        }

        /// <summary>
        /// respond to the SituationClock's request to execute its payload
        /// </summary>
        /// <param name="command"></param>
        public void SituationExecutingRecipe(ISituationEffectCommand command) {
            var tabletopManager = Registry.Retrieve<TabletopManager>();

            //called here in case ongoing slots trigger consumption
            situationWindow.SetSlotConsumptions();

            //move any elements currently in OngoingSlots to situation storage
            //NB we're doing this *before* we execute the command - the command may affect these elements too
            StoreStacks(situationWindow.GetOngoingStacks());

            if (command.AsNewSituation) {
                IVerb verbForNewSituation = compendium.GetOrCreateVerbForCommand(command);
                var scc = new SituationCreationCommand(verbForNewSituation, command.Recipe, SituationState.FreshlyStarted, situationToken as DraggableToken);
                tabletopManager.BeginNewSituation(scc);
                return;
            }

            currentCharacter.AddExecutionsToHistory(command.Recipe.Id, 1);
            var executor = new SituationEffectExecutor();
            executor.RunEffects(command, situationWindow.GetStorageStacksManager(), currentCharacter);

            if (command.Recipe.EndingFlag != null) {
                var ending = compendium.GetEndingById(command.Recipe.EndingFlag);
                tabletopManager.EndGame(ending, this);
            }
        }

        /// <summary>
        /// The situation is complete. DisplayHere the output cards and description
        /// </summary>
        public void SituationComplete() {
            var outputStacks = situationWindow.GetStoredStacks();
            INotification notification = new Notification(SituationClock.GetTitle(), SituationClock.GetDescription());
            SetOutput(outputStacks.ToList());

            situationWindow.ReceiveTextNote(notification);

            //This must be run here: it disables (and destroys) any card tokens that have not been moved to outputs
            situationWindow.SetComplete();

            // Now update the token based on the current stacks in the window
            situationToken.DisplayComplete();
            situationToken.SetCompletionCount(GetNumOutputCards());
            situationToken.DisplayStackInMiniSlot(situationWindow.GetOngoingStacks());

            AttemptAspectInductions();

            var currentRecipe = compendium.GetRecipeById(SituationClock.RecipeId);

            if (currentRecipe.PortalEffect != PortalEffect.None)
                Registry.Retrieve<TabletopManager>().ShowMansusMap(this, ((MonoBehaviour) situationToken).transform, currentRecipe.PortalEffect);
        }

        public void Halt() {
            //currently used only in debug. Reset to starting state (which might be weird for Time) and end timer.
            SituationClock.Halt();
            //If we leave anything in the ongoing slot, it's lost, and also the situation ends up in an anomalous state which breaks loads
            situationWindow.SetOutput(situationWindow.GetOngoingStacks().ToList());
        }

        private void AttemptAspectInductions() {
            //If any elements in the output have inductions, test whether to start a new recipe
            var outputAspects = situationWindow.GetAspectsFromOutputElements(true);

            foreach (var a in outputAspects) {
                var aspectElement = compendium.GetElementById(a.Key);

                if (aspectElement != null)
                    PerformAspectInduction(aspectElement);
                else
                    NoonUtility.Log("unknown aspect " + a + " in output");
            }
        }

        void PerformAspectInduction(Element aspectElement) {
            foreach (var induction in aspectElement.Induces) {
                var d = Registry.Retrieve<IDice>();

                if (d.Rolld100() <= induction.Chance) 
                    CreateRecipeFromInduction(compendium.GetRecipeById(induction.Id), aspectElement.Id);
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
            Registry.Retrieve<TabletopManager>().BeginNewSituation(inducedSituation);
        }

        public void ResetSituation() {
            SituationClock.ResetIfComplete();

            //if this was a transient verb, clean up everything and finish.
            if (situationToken.IsTransient) {
                Retire();
            }
            else {
                situationWindow.SetUnstarted();
                situationToken.SetCompletionCount(-1);
            }
        }

        #endregion

        #region -- SituationClock Window Communication --------------------

        public void OpenWindow() {
            // Make sure we're displaying as unstarted if for some reason we did not reset the window
            if (SituationClock.State == SituationState.Unstarted)
                situationWindow.SetUnstarted();

            IsOpen = true;
            situationToken.DisplayAsOpen();
            situationWindow.Show();
            Registry.Retrieve<TabletopManager>().CloseAllSituationWindowsExcept(situationToken.Id);
        }

        public void CloseWindow() {
            IsOpen = false;
            // This comes first so the token doesn't show a glow when it's being closed
            situationWindow.DumpAllStartingCardsToDesktop(); // only dumps if it can, obv.
            situationWindow.Hide();

            situationToken.DisplayAsClosed();
        }

        public bool CanTakeDroppedToken(IElementStack stack) {
            if (SituationClock.State != SituationState.Unstarted)
                return false;

            return HasEmptyStartingSlots();
        }

        bool HasEmptyStartingSlots() {
            var win = situationWindow as SituationWindow;
            var startSlots = win.GetStartingSlots();

            for (int i = 0; i < startSlots.Count; i++) {
                if (startSlots[i].GetElementStackInSlot() != null)
                    return false;
            }

            return true;
        }

        public bool PushDraggedStackIntoStartingSlots(IElementStack stack) {
            var win = situationWindow as SituationWindow;
            var startSlots = win.GetStartingSlots();

            for (int i = 0; i < startSlots.Count; i++) {
                if (startSlots[i].GetElementStackInSlot() != null)
                    continue; // occupied slot? Go on

                startSlots[i].OnDrop(null); // On drop pushes the currently dragged stack into this slot, with all the movement and parenting
                return true;
            }

            return false;
        }

        public void StartingSlotsUpdated() {
            // it's possible the starting slots have been updated because we've just started a recipe and stored everything that was in them.
            // in this case, don't do anything.
            if (SituationClock.State != SituationState.Unstarted)
                return;

            // Get all aspects and find a recipe
            IAspectsDictionary allAspects = situationWindow.GetAspectsFromAllSlottedElements();
            Recipe matchingRecipe = compendium.GetFirstRecipeForAspectsWithVerb(allAspects, situationToken.Id, currentCharacter, false);

            // Update the aspects in the window
            IAspectsDictionary aspectsNoElementsSelf = situationWindow.GetAspectsFromAllSlottedElements(false);
            situationWindow.DisplayAspects(aspectsNoElementsSelf);

            //if we found a recipe, display it, and get ready to activate
            if (matchingRecipe != null) {
                situationWindow.DisplayStartingRecipeFound(matchingRecipe);

                if (matchingRecipe.MaxExecutions == 1)
                    situationWindow.DisplayRecipeMetaComment("This will only happen once.");
                if (matchingRecipe.MaxExecutions > 1)
                    situationWindow.DisplayRecipeMetaComment("This will only happen " + matchingRecipe.MaxExecutions + " times.");

                return;
            }

            //if we can't find a matching craftable recipe, check for matching hint recipes
            Recipe matchingHintRecipe = compendium.GetFirstRecipeForAspectsWithVerb(allAspects, situationToken.Id, currentCharacter, true); ;

            //perhaps we didn't find an executable recipe, but we did find a hint recipe to display
            if (matchingHintRecipe != null)  
                situationWindow.DisplayHintRecipeFound(matchingHintRecipe);
            //no recipe, no hint? If there are any elements in the mix, display 'try again' message
            else if (allAspects.Count > 0)
                situationWindow.DisplayNoRecipeFound();
            //no recipe, no hint, no aspects. Just set back to unstarted
            else
                situationWindow.SetUnstarted();
        }

        public void OngoingSlotsUpdated() {
            //we don't display the elements themselves, just their aspects.
            //But we *do* take the elements themselves into consideration for determining recipe execution
            var aspectsToDisplayInBottomBar = GetAspectsAvailableToSituation(false);
            var allAspects = GetAspectsAvailableToSituation(true);

            situationWindow.DisplayAspects(aspectsToDisplayInBottomBar);
            var rp = GetNextRecipePrediction(allAspects);

            if (rp != null && rp.BurnImage != null)
                BurnImageUnderToken(rp.BurnImage);

            situationWindow.UpdateTextForPrediction(rp);
            situationToken.DisplayStackInMiniSlot(situationWindow.GetOngoingStacks());
        }

        private RecipePrediction GetNextRecipePrediction(IAspectsDictionary aspects) {
            RecipeConductor rc = new RecipeConductor(compendium, aspects,
                new DefaultDice(), currentCharacter); //nb the use of default dice: we don't want to display any recipes without a 100% chance of executing
            return SituationClock.GetPrediction(rc);
        }

        public void UpdateSituationDisplayForDescription() {
            RecipeConductor rc = new RecipeConductor(compendium, situationWindow.GetAspectsFromAllSlottedAndStoredElements(true), Registry.Retrieve<IDice>(), currentCharacter);

            var nextRecipePrediction = SituationClock.GetPrediction(rc);

            situationWindow.UpdateTextForPrediction(nextRecipePrediction);
        }

        public void SetOutput(List<IElementStack> stacksForOutput) {
            situationWindow.SetOutput(stacksForOutput);
        }

        public void AddNote(INotification notification) {
            situationWindow.ReceiveTextNote(notification);
        }

        public void ShowDestinationsForStack(IElementStack stack, bool show) {
            situationWindow.ShowDestinationsForStack(stack, show);
        }

        public void DumpAllResults() {
            if (SituationClock.State == SituationState.Complete)
                situationWindow.DumpAllResultingCardsToDesktop();
        }

        #endregion

        #region -- External Situation Change Methods --------------------
        // letting other things change the situation

        public void AttemptActivateRecipe() {
            var aspects = situationWindow.GetAspectsFromAllSlottedElements();
            var recipe = compendium.GetFirstRecipeForAspectsWithVerb(aspects, situationToken.Id, currentCharacter, false);

            //no recipe found? get outta here
            if (recipe == null)
                return;

            //kick off the situation. We want to do this first, so that modifying the stacks next won't cause the window to react
            //as if we're removing items from an unstarted situation
            SituationClock.Start(recipe);

            //called here in case starting slots trigger consumption
            situationWindow.SetSlotConsumptions();
            //move any slotted elements to storage
            situationWindow.StoreStacks(situationWindow.GetStartingStacks());

            //The game might be paused! or the player might just be incredibly quick off the mark
            //so immediately continue with a 0 interval - this won't advance time, but will update the visuals in the situation window
            //(which among other things should make the starting slot unavailable

            RecipeConductor rc = new RecipeConductor(compendium,
                GetAspectsAvailableToSituation(true), Registry.Retrieve<IDice>(), currentCharacter);

            SituationClock.Continue(rc, 0);

            //display any burn image the recipe might require

            if (recipe.BurnImage != null)
                BurnImageUnderToken(recipe.BurnImage);
        }

        public void NotifyGreedySlotAnim(TokenAnimationToSlot slotAnim) {
            greedyAnimIsActive = true;
            slotAnim.onElementSlotAnimDone += HandleOnGreedySlotAnimDone;
        }

        void HandleOnGreedySlotAnimDone(ElementStackToken element, TokenAndSlot tokenSlotPair) {
            greedyAnimIsActive = false;
        }

        // Update Visuals

        public void UpdateTokenResultsCountBadge() {
            situationToken.SetCompletionCount(GetNumOutputCards());
        }

        private void BurnImageUnderToken(string burnImage) {
            Registry.Retrieve<INotifier>()
                .ShowImageBurn(burnImage, situationToken as DraggableToken, 20f, 2f,
                    TabletopImageBurner.ImageLayoutConfig.CenterOnToken);
        }

        /// <summary>
        /// This forces the situation to switch to a new recipe - used in debugging
        /// It currently does this by recreating the internal situation, so it may lose state.
        /// </summary>
        /// <param name="recipeId"></param>
        public void OverrideCurrentRecipe(string recipeId)
        {
            var newRecipe = compendium.GetRecipeById(recipeId);

            if (newRecipe == null)
            { 
                NoonUtility.Log("Can't override with recipe id " + recipeId +" because it ain't.");
                return;
            }
            else
                NoonUtility.Log("Overriding with recipe id " + recipeId);

            SituationClock =new SituationClock(SituationClock.TimeRemaining,SituationClock.State,newRecipe,this);

            UpdateSituationDisplayForDescription();
        }

        #endregion

        public IRecipeSlot GetSlotBySaveLocationInfoPath(string locationInfo, string slotType) {
            if (slotType == SaveConstants.SAVE_STARTINGSLOTELEMENTS) //hacky! this should  be an enum or something OOier
                return situationWindow.GetStartingSlotBySaveLocationInfoPath(locationInfo);
            else
                return situationWindow.GetOngoingSlotBySaveLocationInfoPath(locationInfo);
        }

        public Hashtable GetSaveData() {
            var situationSaveData = new Hashtable();
            IGameDataExporter exporter = new GameDataExporter();

            situationSaveData.Add(SaveConstants.SAVE_VERBID, situationToken.Id);
            if (SituationClock != null) {
                situationSaveData.Add(SaveConstants.SAVE_TITLE, situationWindow.Title);
                situationSaveData.Add(SaveConstants.SAVE_RECIPEID, SituationClock.RecipeId);
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONSTATE, SituationClock.State);
                situationSaveData.Add(SaveConstants.SAVE_TIMEREMAINING, SituationClock.TimeRemaining);

                situationSaveData.Add(SaveConstants.SAVE_COMPLETIONCOUNT, GetNumOutputCards());
            }

            //save stacks in window (starting) slots
            if (situationWindow.GetStartingStacks().Any()) {
                var htStartingSlots = exporter.GetHashTableForStacks(situationWindow.GetStartingStacks());
                situationSaveData.Add(SaveConstants.SAVE_STARTINGSLOTELEMENTS, htStartingSlots);
            }

            //save stacks in ongoing slots
            if (situationWindow.GetOngoingStacks().Any()) {
                var htOngoingSlots = exporter.GetHashTableForStacks(situationWindow.GetOngoingStacks());
                situationSaveData.Add(SaveConstants.SAVE_ONGOINGSLOTELEMENTS, htOngoingSlots);
            }

            //save stacks in storage
            if (situationWindow.GetStoredStacks().Any()) {
                var htStacksInStorage = exporter.GetHashTableForStacks(situationWindow.GetStoredStacks());
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONSTOREDELEMENTS, htStacksInStorage);
            }

            //save stacks in output
            if (situationWindow.GetOutputStacks().Any()) {
                var htStacksInOutput = exporter.GetHashTableForStacks(situationWindow.GetOutputStacks());
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS, htStacksInOutput);
            }

            //save notes, and their contents
            if (situationWindow.GetNotes().Any()) {
                var htNotes = exporter.GetHashTableForSituationNotes(situationWindow.GetNotes());
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONNOTES, htNotes);
            }
            return situationSaveData;
        }

        public void SetEditorActive(bool active)
        {
            situationToken.SetEditorActive(active);
        }
    }
}

