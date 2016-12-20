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

        public SituationController(ICompendium c)
        {
            compendium = c;
        }

        public void Initialise(SituationCreationCommand command, ISituationAnchor t, ISituationDetails w)
        {

            situationToken = t;
            t.Initialise(command.GetBasicOrCreatedVerb(), this);

            situationWindow = w;
            w.Initialise(command.GetBasicOrCreatedVerb(), this);

            if (command.Recipe!=null)
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
            AspectsDictionary startingAspects = situationWindow.GetAspectsFromSlottedElements();
            situationWindow.DisplayAspects(startingAspects);

            var r = compendium.GetFirstRecipeForAspectsWithVerb(startingAspects, situationToken.Id);

            situationWindow.DisplayRecipe(r);
        }

        public void OngoingSlotsUpdated()
        {
            var allAspects = GetAspectsAvailableToSituation();
            situationWindow.DisplayAspects(allAspects);

                situationWindow.UpdateSituationDisplay(SituationStateMachine.GetTitle(),
                SituationStateMachine.GetDescription(), getNextRecipePrediction(allAspects));
        
        }


        private string getNextRecipePrediction(IAspectsDictionary aspects)
        {
            
                RecipeConductor rc = new RecipeConductor(compendium, aspects,
                    new Dice());
                return SituationStateMachine.GetPrediction(rc);
            }

    private IAspectsDictionary GetAspectsAvailableToSituation()
       {
           IAspectsDictionary existingAspects = situationToken.GetAspectsFromStoredElements();

            IAspectsDictionary additionalAspects = situationToken.GetAspectsFromSlottedElements();

            IAspectsDictionary allAspects = new AspectsDictionary();
           allAspects.CombineAspects(existingAspects);
           allAspects.CombineAspects(additionalAspects);
           return allAspects;
       }


  

        public HeartbeatResponse ExecuteHeartbeat(float interval)
        {
            HeartbeatResponse response=new HeartbeatResponse();

                RecipeConductor rc = new RecipeConductor(compendium,
                GetAspectsAvailableToSituation(), new Dice());
                SituationStateMachine.Continue(rc, interval);

            if (SituationStateMachine.State==SituationState.Ongoing)
                    response.SlotsToFill = situationToken.GetUnfilledGreedySlots();


            return response;
        }

       public void UpdateSituationDisplayTextInWIndow()
       {
           
            RecipeConductor rc = new RecipeConductor(compendium, situationToken.GetAspectsFromStoredElements(),
            new Dice());

           string nextRecipePrediction = SituationStateMachine.GetPrediction(rc);

            situationWindow.UpdateSituationDisplay(SituationStateMachine.GetTitle(), SituationStateMachine.GetDescription(), nextRecipePrediction);
       }

        public void SituationBeginning(Recipe withRecipe)
        {
            situationToken.DisplaySlotsForSituation(SituationStateMachine.GetSlotsForCurrentRecipe());
            situationWindow.DisplayOngoing(withRecipe);
            UpdateSituationDisplayTextInWIndow();
        }
        public void SituationOngoing()
        {
            situationToken.DisplayTimeRemaining(SituationStateMachine.Warmup, SituationStateMachine.TimeRemaining);
        }


        public void SituationExecutingRecipe(IEffectCommand command)
       {
           //move any elements currently in OngoingSlots to situation storage
           //NB we're doing this *before* we execute the command - the command may affect these elements too
           situationToken.AbsorbOngoingSlotContents();

           if (command.AsNewSituation)
           {
               IVerb verbForNewSituation = new CreatedVerb(command.Recipe.ActionId, command.Recipe.Label,
                   command.Recipe.Description);
               SituationCreationCommand scc = new SituationCreationCommand(verbForNewSituation, command.Recipe);
               Registry.TabletopManager.BeginNewSituation(scc);
           }
           else
           {
               //execute each recipe in command
               foreach (var kvp in command.GetElementChanges())
               {
                   situationToken.ModifyStoredElementStack(kvp.Key, kvp.Value);
               }

               if (command.Recipe.Ending != null)
               {
                   var endingNotification = compendium.GetNotificationForEndingFlag(command.Recipe.Ending);
                   Registry.TabletopManager.EndGame(endingNotification);
               }

            }

        }

        public void SituationComplete()
       {
          //retrieve all stacks stored in the situation
           var stacksToRetrieve = situationToken.GetStoredStacks();
            //create a notification reflecting what just happened
            INotification notification=new Notification(SituationStateMachine.GetTitle(),SituationStateMachine.GetDescription());
            //put all the stacks, and the notification, into the window for player retrieval
            situationWindow.AddOutput(stacksToRetrieve,notification);

            situationToken.SituationComplete();
           
        }

        public void SituationHasBeenReset()
        {
          situationWindow.DisplayStarting();
        }

        public void AddOutput(IEnumerable<IElementStack> stacksForOutput,Notification notification  )
        {
            situationWindow.AddOutput(stacksForOutput, notification);
        }

        public void RecreateSituation(SituationCreationCommand command)
        {
            SituationStateMachine = command.CreateSituationStateMachine(this);
            situationToken.DisplaySlotsForSituation(SituationStateMachine.GetSlotsForCurrentRecipe());
            situationToken.DisplayTimeRemaining(SituationStateMachine.Warmup, SituationStateMachine.TimeRemaining);
            UpdateSituationDisplayTextInWIndow();
        }


        public void AttemptActivateRecipe()
       {
            var aspects =situationWindow.GetAspectsFromSlottedElements();
            var recipe = compendium.GetFirstRecipeForAspectsWithVerb(aspects, situationToken.Id);
            if (recipe != null)
            {
                situationWindow.RunSlotConsumptions();
                situationToken.StoreStacks(situationWindow.GetStacksInStartingSlots());
                SituationStateMachine.Start(recipe);
            }
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

              

       }

        public Hashtable GetSaveDataForSituation()
        {
           var situationSaveData=new Hashtable();
            var exporter = new GameDataExporter();

            situationSaveData.Add(SaveConstants.SAVE_VERBID, situationToken.Id);
            if (SituationStateMachine != null)
            {
                situationSaveData.Add(SaveConstants.SAVE_RECIPEID, SituationStateMachine.RecipeId);
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONSTATE, SituationStateMachine.State);
                situationSaveData.Add(SaveConstants.SAVE_TIMEREMAINING, SituationStateMachine.TimeRemaining);
            }

            
            //save stacks in window (starting) slots
            if(situationWindow.GetStacksInStartingSlots().Any())
            { 
            var htStartingSlots= exporter.GetHashTableForStacks(situationWindow.GetStacksInStartingSlots());
            situationSaveData.Add(SaveConstants.SAVE_STARTINGSLOTELEMENTS,htStartingSlots);
            }

            //save stacks in ongoing slots
            if (situationToken.GetStacksInOngoingSlots().Any())
            { 
            var htOngoingSlots = exporter.GetHashTableForStacks(situationToken.GetStacksInOngoingSlots());
            situationSaveData.Add(SaveConstants.SAVE_ONGOINGSLOTELEMENTS,htOngoingSlots);
            }

            //save stacks in storage
            if(situationToken.GetStoredStacks().Any())
            { 
            var htStacksInStorage = exporter.GetHashTableForStacks(situationToken.GetStoredStacks());
            situationSaveData.Add(SaveConstants.SAVE_SITUATIONSTOREDELEMENTS,htStacksInStorage);
            }

            //save notes, and their contents
            if(situationWindow.GetCurrentOutputs().Any())
            { 
            var htOutputs = exporter.GetHashtableForOutputNotes(situationWindow.GetCurrentOutputs());
            situationSaveData.Add(SaveConstants.SAVE_SITUATIONOUTPUTS,htOutputs);
            }
            return situationSaveData;                
                
            }

        public IRecipeSlot GetSlotBySaveLocationInfoPath(string locationInfo,string slotType)
        {
            if (slotType==SaveConstants.SAVE_STARTINGSLOTELEMENTS) //hacky! this should  be an enum or something OOier
                return situationWindow.GetStartingSlotBySaveLocationInfoPath(locationInfo);
            else
                return situationToken.GetOngoingSlotBySaveLocationInfoPath(locationInfo);
        }

    }
    }

