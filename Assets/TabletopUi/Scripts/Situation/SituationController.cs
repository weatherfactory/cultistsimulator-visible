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
    public class SituationController : ISituationSubscriber,ISaveable {
        public ISituationAnchor situationToken;
        private ISituationDetails situationWindow;
        public ISituation Situation;
        private readonly ICompendium compendium;
        private readonly Character currentCharacter;
        public bool IsOpen { get; set; }

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

            Situation = new Situation(command.TimeRemaining, command.State, command.Recipe, this);

            if (command.State == SituationState.Unstarted) {
                situationWindow.SetUnstarted();
            }
            else if (command.State == SituationState.FreshlyStarted || command.State == SituationState.Ongoing || command.State == SituationState.RequiringExecution) {
                if (command.State == SituationState.FreshlyStarted)
                    Situation.Start(command.Recipe);
                //ugly subclause here. Situation.Start largely duplicates the constructor. I'm trying to use the same code path for recreating situations from a save file as for beginning a new situation
                //possibly just separating out FreshlyStarted would solve it

                situationWindow.SetOngoing(command.Recipe);

                situationToken.DisplayMiniSlotDisplay(command.Recipe.SlotSpecifications);
                situationToken.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining, command.Recipe);
                situationWindow.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining, command.Recipe);


                //this is a little ugly here; but it makes the intent clear. The best way to deal with it is probably to pass the whole Command down to the situationwindow for processing.
                if (command.OverrideTitle != null)
                    situationWindow.Title = command.OverrideTitle;

            }
            else if (command.State == SituationState.Complete) {
                Situation = new Situation(this);
                Situation.State = SituationState.Complete;
                situationWindow.SetComplete();

                //this is a little ugly here; but it makes the intent clear. The best way to deal with it is probably to pass the whole Command down to the situationwindow for processing.
                if (command.OverrideTitle != null)
                    situationWindow.Title = command.OverrideTitle;
                //NOTE: only on Complete state. Completioncount shouldn't show on other states. This is fragile tho.
                if (command.CompletionCount > 0)
                    situationToken.SetCompletionCount(command.CompletionCount);

            }
            else {
                throw new ApplicationException("Tried to create situation for " + command.Verb.Label +
                                               " with unknown state");
            }
        }

        public void OpenSituation() {
            // Make sure we're displaying as unstarted if for some reason we did not reset the window
            if (Situation.State == SituationState.Unstarted)
                situationWindow.SetUnstarted();

            IsOpen = true;
            situationToken.OpenToken();
            situationWindow.Show();
        }

        public void CloseSituation() {
            // This comes first so the token doesn't show a glow when it's being closed
            situationWindow.DumpAllStartingCardsToDesktop(); // only dumps if it can, obv.
            situationToken.CloseToken();
            situationWindow.Hide();
        }

        public string GetCurrentRecipeId() {
            return Situation == null ? null : Situation.RecipeId;
        }

        public string GetActionId()
        {
            return situationToken.Id;
        }

        public bool CanTakeDroppedToken(IElementStack stack) {
            if (Situation.State != SituationState.Unstarted)
                return false;

            return HasEmptyStartingSlots();
        }

        public bool HasEmptyStartingSlots() {
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
            //it's possible the starting slots have been updated because we've just started a recipe and stored everything that was in them.
            //in this case, don't do anything.
            if (Situation.State != SituationState.Unstarted)
                return;
            else
            {
                //This is an unstarted situation: start displaying recipes, hints, whatnot
              //get all the aspects currently in the starting slots
            
            IAspectsDictionary allAspects = situationWindow.GetAspectsFromAllSlottedElements();
            Recipe hintRecipeMatchingStartingAspects=null;
                var recipeMatchingStartingAspects = compendium.GetFirstRecipeForAspectsWithVerb(allAspects, situationToken.Id, currentCharacter,false);

            //if we can't find a matching craftable recipe, check for matching hint recipes
            if (recipeMatchingStartingAspects==null)
                hintRecipeMatchingStartingAspects =compendium.GetFirstRecipeForAspectsWithVerb(allAspects, situationToken.Id, currentCharacter, true);

            IAspectsDictionary aspectsNoElementsSelf = situationWindow.GetAspectsFromAllSlottedElements(false);

            situationWindow.DisplayAspects(aspectsNoElementsSelf);

            //if we found a recipe, display it, and get ready to activate
            if (recipeMatchingStartingAspects != null)
            {
                situationWindow.DisplayStartingRecipeFound(recipeMatchingStartingAspects);
                if (recipeMatchingStartingAspects.MaxExecutions == 1)
                   situationWindow.DisplayRecipeMetaComment("This will only happen once.");
                if (recipeMatchingStartingAspects.MaxExecutions > 1)
                    situationWindow.DisplayRecipeMetaComment("This will only happen " +recipeMatchingStartingAspects.MaxExecutions + " times.");
            }
            //perhaps we didn't find an executable recipe, but we did find a hint recipe to display
            else if (hintRecipeMatchingStartingAspects!= null)
                situationWindow.DisplayHintRecipeFound(hintRecipeMatchingStartingAspects);
            //no recipe, no hint? If there are any elements in the mix, display 'try again' message
            else if (allAspects.Count > 0)
                situationWindow.DisplayNoRecipeFound();
            //no recipe, no hint, no aspects. Just set back to unstarted
            else
                situationWindow.SetUnstarted();
            }
        }

        public void OngoingSlotsUpdated() {
            //we don't display the elements themselves, just their aspects.
            //But we *do* take the elements themselves into consideration for determining recipe execution
            var aspectsToDisplayInBottomBar = GetAspectsAvailableToSituation(false);
            var allAspects = GetAspectsAvailableToSituation(true);

            situationWindow.DisplayAspects(aspectsToDisplayInBottomBar);
            var rp = getNextRecipePrediction(allAspects);

            if (rp != null && rp.BurnImage != null)
                BurnImageHere(rp.BurnImage);

            situationWindow.UpdateTextForPrediction(rp);
            situationToken.UpdateMiniSlotDisplay(situationWindow.GetOngoingStacks());
        }

        private RecipePrediction getNextRecipePrediction(IAspectsDictionary aspects) {
            RecipeConductor rc = new RecipeConductor(compendium, aspects,
                new DefaultDice(), currentCharacter); //nb the use of default dice: we don't want to display any recipes without a 100% chance of executing
            return Situation.GetPrediction(rc);
        }

        private IAspectsDictionary GetAspectsAvailableToSituation(bool showElementAspects)
        {
            var aspects = situationWindow.GetAspectsFromAllSlottedElements(showElementAspects);
            aspects.CombineAspects(situationWindow.GetAspectsFromStoredElements(showElementAspects));
            return aspects;
        }

        public HeartbeatResponse ExecuteHeartbeat(float interval) {
            HeartbeatResponse response = new HeartbeatResponse();

            RecipeConductor rc = new RecipeConductor(compendium,
                GetAspectsAvailableToSituation(true), Registry.Retrieve<IDice>(), currentCharacter);

            Situation.Continue(rc, interval);

            if (Situation.State == SituationState.Ongoing) {
                var tokenAndSlot = new TokenAndSlot() {
                    Token = situationToken as SituationToken,
                    RecipeSlot = situationWindow.GetUnfilledGreedySlot() as RecipeSlot
                };

                if (tokenAndSlot.RecipeSlot != null)
                    response.SlotsToFill.Add(tokenAndSlot);
            }

            return response;
        }

        public void UpdateSituationDisplayForDescription() {
            RecipeConductor rc = new RecipeConductor(compendium, situationWindow.GetAspectsFromAllSlottedElements(), Registry.Retrieve<IDice>(), currentCharacter);

            var nextRecipePrediction = Situation.GetPrediction(rc);

            situationWindow.UpdateTextForPrediction(nextRecipePrediction);
        }

        public void SituationBeginning(Recipe withRecipe) {
            
            situationToken.UpdateMiniSlotDisplay(null); // Hide content of miniSlotDisplay - looping recipes never go by complete which would do that
            situationToken.DisplayMiniSlotDisplay(withRecipe.SlotSpecifications);
            situationWindow.SetOngoing(withRecipe);
            StoreStacks(situationWindow.GetStartingStacks());

            UpdateSituationDisplayForDescription();

            if (withRecipe.EndsGame())
            {
                var tabletopManager = Registry.Retrieve<TabletopManager>();
                tabletopManager.SignalImpendingDoom(situationToken);
            }
        }

        public void SituationOngoing()
        {
            var currentRecipe = compendium.GetRecipeById(Situation.RecipeId);
            situationToken.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining, currentRecipe);
            situationWindow.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining, currentRecipe);
        }

        void StoreStacks(IEnumerable<IElementStack> stacks) {
            var inputStacks = situationWindow.GetOngoingStacks();
            var storageStackManager = situationWindow.GetStorageStacksManager();
            storageStackManager.AcceptStacks(inputStacks);
            situationWindow.DisplayStoredElements(); //displays the miniversion of the cards. This should 
        }

        /// <summary>
        /// respond to the Situation's request to execute its payload
        /// </summary>
        /// <param name="command"></param>
        public void SituationExecutingRecipe(ISituationEffectCommand command)
        {
            //called here in case ongoing slots trigger consumption
            situationWindow.SetSlotConsumptions();

            //move any elements currently in OngoingSlots to situation storage
            //NB we're doing this *before* we execute the command - the command may affect these elements too
            StoreStacks(situationWindow.GetOngoingStacks());

            if (command.AsNewSituation) {
                IVerb verbForNewSituation = compendium.GetOrCreateVerbForCommand(command);
                SituationCreationCommand scc = new SituationCreationCommand(verbForNewSituation, command.Recipe, SituationState.FreshlyStarted, situationToken as DraggableToken);
                Registry.Retrieve<TabletopManager>().BeginNewSituation(scc);
                return;
            }

            var tabletopManager = Registry.Retrieve<TabletopManager>();


            currentCharacter.AddExecutionsToHistory(command.Recipe.Id,1);
            var executor = new SituationEffectExecutor();
            executor.RunEffects(command, situationWindow.GetStorageStacksManager(),currentCharacter);

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
            INotification notification = new Notification(Situation.GetTitle(), Situation.GetDescription());
            SetOutput(outputStacks.ToList());

            situationWindow.ReceiveNotification(notification);

            //This must be run here: it disables (and destroys) any card tokens that have not been moved to outputs
            situationWindow.SetComplete();

            // Now update the token based on the current stacks in the window
            situationToken.DisplayComplete();
            situationToken.UpdateMiniSlotDisplay(situationWindow.GetOngoingStacks());

            AttemptAspectInductions();
        }

        public void Halt()
        {
            //currently used only in debug. Reset to starting state (which might be weird for Time) and end timer.
           Situation.Halt();
           

        }

        private void AttemptAspectInductions()
        {
//If any elements in the output have inductions, test whether to start a new recipe
            var outputAspects = situationWindow.GetAspectsFromOutputElements(true);

            foreach (var a in outputAspects)
            {
                var aspectElement = compendium.GetElementById(a.Key);
                if (aspectElement == null)
                    NoonUtility.Log("unknown aspect " + a + " in output");
                else
                {
                    foreach (var induction in aspectElement.Induces)
                    {
                        var d = Registry.Retrieve<IDice>();
                        if (d.Rolld100() <= induction.Chance)
                        {
                            var inducedRecipe = compendium.GetRecipeById(induction.Id);
                            if (inducedRecipe == null)
                                NoonUtility.Log("unknown recipe " + inducedRecipe + " in induction for " + aspectElement.Id);
                            else
                            {
                                var inductionRecipeVerb = new CreatedVerb(inducedRecipe.ActionId,
                                    inducedRecipe.Label, inducedRecipe.Description);
                                SituationCreationCommand inducedSituation = new SituationCreationCommand(inductionRecipeVerb,
                                    inducedRecipe, SituationState.FreshlyStarted, situationToken as DraggableToken);
                                Registry.Retrieve<TabletopManager>().BeginNewSituation(inducedSituation);
                            }
                        }
                    }
                }
            }
        }

        public void SituationHasBeenReset() {
            situationWindow.SetUnstarted();
            ResetToStartingState();
        }

        public void SetOutput(List<IElementStack> stacksForOutput) {
            situationWindow.SetOutput(stacksForOutput);
        }

        public void AddNote(INotification notification)
        {
            situationWindow.ReceiveNotification(notification);
        }

        public void UpdateTokenResultsCountBadge() {
            situationToken.SetCompletionCount(GetNumOutputCards());
        }

        public int GetNumOutputCards() {
            int count = 0;
            var stacks = situationWindow.GetOutputStacks();

            foreach (var item in stacks) {
                if (item.Defunct)
                    continue;

                count += item.Quantity;
            }

            return count;
        }

        public void AttemptActivateRecipe() {
            var aspects = situationWindow.GetAspectsFromAllSlottedElements();
            var recipe = compendium.GetFirstRecipeForAspectsWithVerb(aspects, situationToken.Id, currentCharacter,false);

            //no recipe found? get outta here
            if (recipe == null)
                return;

            //kick off the situation. We want to do this first, so that modifying the stacks next won't cause the window to react
            //as if we're removing items from an unstarted situation
            Situation.Start(recipe);

            //called here in case starting slots trigger consumption
            situationWindow.SetSlotConsumptions();
            //move any slotted elements to storage
            situationWindow.StoreStacks(situationWindow.GetStartingStacks());
            

            //The game might be paused! or the player might just be incredibly quick off the mark
            //so immediately continue with a 0 interval - this won't advance time, but will update the visuals in the situation window
            //(which among other things should make the starting slot unavailable

            RecipeConductor rc = new RecipeConductor(compendium,
                GetAspectsAvailableToSituation(true), Registry.Retrieve<IDice>(), currentCharacter);

            Situation.Continue(rc,0);

            //display any burn image the recipe might require

            if (recipe.BurnImage != null)
                BurnImageHere(recipe.BurnImage);
        }

        private void BurnImageHere(string burnImage) {
            Registry.Retrieve<INotifier>()
                .ShowImageBurn(burnImage, situationToken as DraggableToken, 20f, 2f,
                    TabletopImageBurner.ImageLayoutConfig.CenterOnToken);
        }

        public void ModifyStoredElementStack(string elementId, int quantity) {
            situationWindow.GetStorageStacksManager().ModifyElementQuantity(elementId, quantity, Source.Existing());
            situationWindow.DisplayStoredElements();
        }

        public void ResetToStartingState() {
            Situation.ResetIfComplete();
            //if this was a transient verb, clean up everything and finish.
            //otherwise, prep the window for the next recipe
            if (situationToken.IsTransient) {
                situationToken.Retire();
                situationWindow.Retire();
                //at the moment, the controller is accessed through the token
                //if we attach the controller to a third object, we'd need to retire that too
                //...and here that third thing is! but we're still in mid-refactor.
                Registry.Retrieve<SituationsCatalogue>().DeregisterSituation(this);
            }
            else {
                situationWindow.SetUnstarted();
                situationToken.SetCompletionCount(0);
            }
        }

        public void Retire()
        {
            situationToken.Retire();
            Registry.Retrieve<SituationsCatalogue>().DeregisterSituation(this);
        }

        public Hashtable GetSaveData() {
            var situationSaveData = new Hashtable();
            IGameDataExporter exporter = new GameDataExporter();

            situationSaveData.Add(SaveConstants.SAVE_VERBID, situationToken.Id);
            if (Situation != null) {
                situationSaveData.Add(SaveConstants.SAVE_TITLE, situationWindow.Title);
                situationSaveData.Add(SaveConstants.SAVE_RECIPEID, Situation.RecipeId);
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONSTATE, Situation.State);
                situationSaveData.Add(SaveConstants.SAVE_TIMEREMAINING, Situation.TimeRemaining);

                situationSaveData.Add(SaveConstants.SAVE_COMPLETIONCOUNT, situationToken.GetCompletionCount());
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

        public IRecipeSlot GetSlotBySaveLocationInfoPath(string locationInfo, string slotType) {
            if (slotType == SaveConstants.SAVE_STARTINGSLOTELEMENTS) //hacky! this should  be an enum or something OOier
                return situationWindow.GetStartingSlotBySaveLocationInfoPath(locationInfo);
            else
                return situationWindow.GetOngoingSlotBySaveLocationInfoPath(locationInfo);
        }

        public bool IsSituationOccupied() {
            // This should return true when the situation is occupied:
            /*
            - if the situation isn't currently executing and the primary slot contains an element, it's occupied
            - if the situation is currently executing, or the primary slot doesn't contain an element, it's occupied
            */

            var stacks = situationWindow.GetOutputStacks();

            foreach (var stack in stacks) {
                if (!stack.Defunct)
                    return true;
            }

            return false;
        }

        public int GetElementCountInSituation(string elementId) {
            int count = 0;

            count += GetElementCountFromStack(elementId, situationWindow.GetStartingStacks());
            count += GetElementCountFromStack(elementId, situationWindow.GetOngoingStacks());
            count += GetElementCountFromStack(elementId, situationWindow.GetStoredStacks());
            count += GetElementCountFromStack(elementId, situationWindow.GetOutputStacks());

            return count;
        }

        private int GetElementCountFromStack(string elementId, IEnumerable<IElementStack> stack) {
            int count = 0;

            foreach (var card in stack) {
                // This will NOT count cards that are still face down.
                if (!card.Defunct && card.Id == elementId && card.IsFront())
                    count += card.Quantity;
            }

            return count;
        }

        public void ShowDestinationsForStack(IElementStack stack) {
            situationWindow.ShowDestinationsForStack(stack);
        }
    }
}

