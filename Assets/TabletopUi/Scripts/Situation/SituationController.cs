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

        public ISituationAnchor situationAnchor;
        public SituationWindow situationWindow;
        public Situation Situation;

        





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
            slot.TryPutElementStackInSlot(stack);

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
        
            Recipe matchingRecipe = compendium.GetFirstMatchingRecipe(aspectsInContext,  situationAnchor.EntityId, currentCharacter, false);

            // Update the aspects in the window
            IAspectsDictionary aspectsNoElementsSelf = situationWindowAsStorage.GetAspectsFromAllSlottedElements(false);
            situationWindowAsView.DisplayAspects(aspectsNoElementsSelf);

            //if we found a recipe, display it, and get ready to activate
            if (matchingRecipe != null) {


                situationWindowAsView.DisplayStartingRecipeFound(matchingRecipe);
                return;
            }

            //if we can't find a matching craftable recipe, check for matching hint recipes
            Recipe matchingHintRecipe = compendium.GetFirstMatchingRecipe(aspectsInContext, situationAnchor.EntityId, currentCharacter, true); ;

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

            SituationEventData e=SituationEventData.Create(Situation);
            situationAnchor.ContainerContentsUpdated(e); //this duplicates a potential call via the subscription model

            TryOverrideVerbIcon(situationWindowAsStorage.GetAspectsFromStoredElements(true)); //this also duplicates
        }




        public void SetOutput(List<ElementStackToken> stacksForOutput) {
            situationWindowAsStorage.SetOutput(stacksForOutput);
        }

        public void AddNote(INotification notification) {
            situationWindowAsStorage.ReceiveTextNote(notification);
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


            var recipe = compendium.GetFirstMatchingRecipe(aspectsInContext, situationAnchor.EntityId, currentCharacter, false);

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




        


        public RecipeSlot GetSlotBySaveLocationInfoPath(string locationInfo, string slotType) {
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

            situationSaveData.Add(SaveConstants.SAVE_VERBID, situationAnchor.EntityId);
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

