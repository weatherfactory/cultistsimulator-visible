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

        public void InitialiseToken(ISituationAnchor t, IVerb v)
        {
            situationToken = t;
            t.Initialise(v, this);
        }

        public void InitialiseWindow(ISituationDetails w)
        {
            situationWindow = w;
            w.Initialise(this);
        }


        public void OpenSituation()
        {
        
            situationToken.OpenToken();

            situationWindow.Show(SituationStateMachine!=null);
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

                situationWindow.DisplaySituation(SituationStateMachine.GetTitle(),
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
            if (SituationStateMachine != null)
            {
                RecipeConductor rc = new RecipeConductor(compendium,
                GetAspectsAvailableToSituation(), new Dice());
                SituationStateMachine.Continue(rc, interval);
                response.SlotsToFill = situationToken.GetUnfilledGreedySlots();
            }

            return response;
        }

       public void SituationBeginning(SituationStateMachine s)
       {
            situationToken.SituationBeginning(s.GetSlots());
            SituationOngoing(s);

            RecipeConductor rc = new RecipeConductor(compendium, situationToken.GetAspectsFromStoredElements(),
            new Dice());

           string nextRecipePrediction = SituationStateMachine.GetPrediction(rc);


            situationWindow.DisplaySituation(s.GetTitle(),s.GetDescription(), nextRecipePrediction);
       }

        public void SituationOngoing(SituationStateMachine s)
        {
            situationToken.DisplayTimeRemaining(s.Warmup, s.TimeRemaining);
        }



        public void SituationExecutingRecipe(IEffectCommand command)
       {
            //move any elements currently in OngoingSlots to situation storage
            //NB we're doing this *before* we execute the command - the command may affect these elements too
           situationToken.AbsorbOngoingSlotContents();


            //execute each recipe in command
            foreach (var kvp in command.GetElementChanges())
            {
                situationToken.ModifyStoredElementStack(kvp.Key,kvp.Value);
            }

        }

       public void SituationExtinct()
       {
          //retrieve all stacks stored in the situation
           var stacksToRetrieve = situationToken.GetStoredStacks();
            //create a notification reflecting what just happened
            INotification notification=new Notification(SituationStateMachine.GetTitle(),SituationStateMachine.GetDescription());
            //put all the stacks, and the notification, into the window for player retrieval
            situationWindow.AddOutput(stacksToRetrieve,notification);

            situationToken.SituationExtinct();

            //and finally, the situation is gone
            SituationStateMachine = null;
           
        }

        public void BeginSituation(Recipe r)
        {
            SituationStateMachine = new SituationStateMachine(r);
            SituationStateMachine.Subscribe(this);

        }


        public void AttemptActivateRecipe()
       {
            var aspects =situationWindow.GetAspectsFromSlottedElements();
            var recipe = compendium.GetFirstRecipeForAspectsWithVerb(aspects, situationToken.Id);
            if (recipe != null)
            {
                situationToken.StoreStacks(situationWindow.GetStacksInStartingSlots());

                BeginSituation(recipe);
                situationWindow.Show(true);
            }
        }

       public void AllOutputsGone()
       {
           //if this was a transient verb, clean up everything and finish.
           //otherwise, prep the window for the next recipe
           if (situationToken.IsTransient)
           {
               situationToken.Retire();
               situationWindow.Retire();
                //at the moment, the controller is accessed through the token
               //if we attach the controller to a third object, we'd need to retire that too
           }
            else
              situationWindow.DisplayStarting();

       }

        public void PopulateSaveInfo(IDictionary saveInfo)
        {
            saveInfo.Add(NoonConstants.SAVE_VERBID, situationToken.Id);
            if(SituationStateMachine!=null)
            { 
            saveInfo.Add(NoonConstants.SAVE_RECIPEID, SituationStateMachine.RecipeId);
            }
        }
    }
}
