using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.TabletopUi
{
    public class SituationController : ISituationStateMachineSituationSubscriber
    {
        public ISituationAnchor situationToken;
        private ISituationDetails situationWindow;
        public ISituationStateMachine SituationStateMachine;
        private readonly ICompendium compendium;
        private readonly Character currentCharacter;

        public SituationController(ICompendium co,Character ch)
        {
            compendium = co;
            currentCharacter = ch;
        }

        public void Initialise(SituationCreationCommand command, ISituationAnchor t, ISituationDetails w)
        {
            situationToken = t;
            situationToken.Initialise(command.GetBasicOrCreatedVerb(), this);

            situationWindow = w;
            situationWindow.Initialise(command.GetBasicOrCreatedVerb(), this);
            situationWindow.SetStarting();

            if (command.Recipe != null)
                RecreateSituation(command);
            else
                SituationStateMachine = new SituationStateMachine(this);
        }


        public void OpenSituation()
        {
            situationToken.OpenToken();

            situationWindow.Show();
        }


        public void CloseSituation()
        {
            situationToken.CloseToken();
            situationWindow.Hide();
        }


        public string GetCurrentRecipeId()
        {
            return SituationStateMachine == null ? null : SituationStateMachine.RecipeId;
        }

        public void StartingSlotsUpdated()
        {
            AspectsDictionary startingAspects = situationWindow.GetAspectsFromAllSlottedElements();
            situationWindow.DisplayAspects(startingAspects);


            var r = compendium.GetFirstRecipeForAspectsWithVerb(startingAspects, situationToken.Id,currentCharacter);

            situationWindow.UpdateTextForCandidateRecipe(r);
        }

        public void OngoingSlotsUpdated()
        {
            var allAspects = GetAspectsAvailableToSituation();
            situationWindow.DisplayAspects(allAspects);
            var rp = getNextRecipePrediction(allAspects);

            situationWindow.UpdateTextForPrediction(rp);
            situationToken.UpdateMiniSlotDisplay(situationWindow.GetStacksInOngoingSlots());
        }


        private RecipePrediction getNextRecipePrediction(IAspectsDictionary aspects)
        {
            RecipeConductor rc = new RecipeConductor(compendium, aspects,
                new DefaultDice(), currentCharacter); //nb the use of default dice: we don't want to display any recipes without a 100% chance of executing
            return SituationStateMachine.GetPrediction(rc);
        }

        private IAspectsDictionary GetAspectsAvailableToSituation()
        {
            IAspectsDictionary existingAspects = situationWindow.GetAspectsFromStoredElements();

            IAspectsDictionary additionalAspects = situationWindow.GetAspectsFromAllSlottedElements();

            IAspectsDictionary allAspects = new AspectsDictionary();
            allAspects.CombineAspects(existingAspects);
            allAspects.CombineAspects(additionalAspects);
            return allAspects;
        }


        public HeartbeatResponse ExecuteHeartbeat(float interval)
        {
            HeartbeatResponse response = new HeartbeatResponse();


            RecipeConductor rc = new RecipeConductor(compendium,
                GetAspectsAvailableToSituation(), new Dice(), currentCharacter);
            SituationStateMachine.Continue(rc, interval);

            if (SituationStateMachine.State == SituationState.Ongoing)
            {
                var pair = new TokenAndSlot()
                {
                    Token = situationToken as SituationToken,
                    RecipeSlot = situationWindow.GetUnfilledGreedySlot() as RecipeSlot
                };
                if(pair.RecipeSlot!=null)
                    response.SlotsToFill.Add(pair);

            }

            return response;
        }

        public void UpdateSituationDisplayTextInWIndow()
        {

            RecipeConductor rc = new RecipeConductor(compendium, situationWindow.GetAspectsFromStoredElements(),
                new Dice(),currentCharacter);

            var nextRecipePrediction = SituationStateMachine.GetPrediction(rc);

            situationWindow.UpdateTextForPrediction(nextRecipePrediction);
        }

        public void SituationBeginning(Recipe withRecipe)
        {
            situationToken.DisplayMiniSlotDisplay(withRecipe.SlotSpecifications);
            situationWindow.SetOngoing(withRecipe);
            UpdateSituationDisplayTextInWIndow();
        }

        public void SituationOngoing()
        {
            situationToken.DisplayTimeRemaining(SituationStateMachine.Warmup, SituationStateMachine.TimeRemaining);
        }


        public void SituationExecutingRecipe(ISituationEffectCommand command)
        {
            //move any elements currently in OngoingSlots to situation storage
            //NB we're doing this *before* we execute the command - the command may affect these elements too
            var inputStacks = situationWindow.GetStacksInOngoingSlots();
            var storageGateway = situationWindow.GetSituationStorageStacksManager();
            storageGateway.AcceptStacks(inputStacks);

            if (command.AsNewSituation)
            {

                IVerb verbForNewSituation = compendium.GetOrCreateVerbForCommand(command);

				SituationCreationCommand scc = new SituationCreationCommand(verbForNewSituation, command.Recipe, situationToken as DraggableToken);
                Registry.Retrieve<TabletopManager>().BeginNewSituation(scc);
            }
            else
            {
                
                currentCharacter.AddExecutionToHistory(command.Recipe.Id);
                foreach (var kvp in command.GetElementChanges())
                {
                    situationWindow.GetSituationStorageStacksManager().ModifyElementQuantity(kvp.Key, kvp.Value);
                }

                if (command.Recipe.Ending != null)
                {
                    var endingNotification = compendium.GetNotificationForEndingFlag(command.Recipe.Ending);
                    Registry.Retrieve<TabletopManager>().EndGame(endingNotification);
                }
            }
            
        }

        public void SituationComplete()
        {
            situationToken.DisplayComplete();
            

            var stacksToRetrieve = situationWindow.GetStoredStacks();
            INotification notification = new Notification(SituationStateMachine.GetTitle(),
                SituationStateMachine.GetDescription());
                SetOutput(stacksToRetrieve, notification);
                situationToken.ShowCompletionCount(1);

            //This must be run last: it disables (and destroys) any card tokens that have not been moved to outputs
            situationWindow.SetComplete();
        }

        public void SituationHasBeenReset()
        {
            situationWindow.SetStarting();
        }

        public void SetOutput(IEnumerable<IElementStack> stacksForOutput, INotification notification)
        {
            situationWindow.SetOutput(stacksForOutput, notification);
        }

        public void RecreateSituation(SituationCreationCommand command)
        {
            SituationStateMachine = command.CreateSituationStateMachine(this);
            situationToken.DisplayMiniSlotDisplay(command.Recipe.SlotSpecifications);
            situationToken.DisplayTimeRemaining(SituationStateMachine.Warmup, SituationStateMachine.TimeRemaining);
        }


        public void AttemptActivateRecipe()
        {
            var aspects = situationWindow.GetAspectsFromAllSlottedElements();
            var recipe = compendium.GetFirstRecipeForAspectsWithVerb(aspects, situationToken.Id, currentCharacter);
            if (recipe != null)
            {
                situationWindow.SetSlotConsumptions();
                situationWindow.StoreStacks(situationWindow.GetStacksInStartingSlots());
                SituationStateMachine.Start(recipe);
            }
        }

        public void ModifyStoredElementStack(string elementId, int quantity)
        {
            situationWindow.GetSituationStorageStacksManager().ModifyElementQuantity(elementId, quantity);
        }

        public void AllOutputsGone()
        {
            SituationStateMachine.AllOutputsGone();
            //if this was a transient verb, clean up everything and finish.
            //otherwise, prep the window for the next recipe
            if (situationToken.IsTransient)
            {
                situationToken.Retire();
                situationWindow.Retire();
                //at the moment, the controller is accessed through the token
                //if we attach the controller to a third object, we'd need to retire that too
            }

            situationToken.ShowCompletionCount(0);
        }

        public Hashtable GetSaveDataForSituation()
        {
            var situationSaveData = new Hashtable();
            var exporter = new GameDataExporter();

            situationSaveData.Add(SaveConstants.SAVE_VERBID, situationToken.Id);
            if (SituationStateMachine != null)
            {
                situationSaveData.Add(SaveConstants.SAVE_RECIPEID, SituationStateMachine.RecipeId);
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONSTATE, SituationStateMachine.State);
                situationSaveData.Add(SaveConstants.SAVE_TIMEREMAINING, SituationStateMachine.TimeRemaining);
            }


            //save stacks in window (starting) slots
            if (situationWindow.GetStacksInStartingSlots().Any())
            {
                var htStartingSlots = exporter.GetHashTableForStacks(situationWindow.GetStacksInStartingSlots());
                situationSaveData.Add(SaveConstants.SAVE_STARTINGSLOTELEMENTS, htStartingSlots);
            }

            //save stacks in ongoing slots
            if (situationWindow.GetStacksInOngoingSlots().Any())
            {
                var htOngoingSlots = exporter.GetHashTableForStacks(situationWindow.GetStacksInOngoingSlots());
                situationSaveData.Add(SaveConstants.SAVE_ONGOINGSLOTELEMENTS, htOngoingSlots);
            }

            //save stacks in storage
            if (situationWindow.GetStoredStacks().Any())
            {
                var htStacksInStorage = exporter.GetHashTableForStacks(situationWindow.GetStoredStacks());
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONSTOREDELEMENTS, htStacksInStorage);
            }

            //save notes, and their contents
            if (situationWindow.GetCurrentOutputs().Any())
            {
                var htOutputs = exporter.GetHashtableForOutputNotes(situationWindow.GetCurrentOutputs());
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONOUTPUTS, htOutputs);
            }
            return situationSaveData;
        }

        public IRecipeSlot GetSlotBySaveLocationInfoPath(string locationInfo, string slotType)
        {
            if (slotType == SaveConstants.SAVE_STARTINGSLOTELEMENTS) //hacky! this should  be an enum or something OOier
                return situationWindow.GetStartingSlotBySaveLocationInfoPath(locationInfo);
            else
                return situationWindow.GetOngoingSlotBySaveLocationInfoPath(locationInfo);
        }

        public bool IsSituationOccupied() {
            // This should return true when the situation is occupied:
            /*
            - if the situation isn't currently executing and the primary slot contains an element, it's dirty
            - if the situation is currently executing, or the primary slot doesn't contain an element, it's clean
            */

            return false;
        }
    }
}

