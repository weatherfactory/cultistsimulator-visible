using System;
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
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.TabletopUi
{
   public  class SituationController:ISituationStateMachineSituationSubscriber
   {
       public ISituationAnchor situationToken;
       private ISituationDetails situationWindow;
       public SituationStateMachine SituationStateMachine;
        
       public void InitialiseToken(SituationToken t,IVerb v)
       {
            situationToken = t;
            t.Initialise(v, this);
        }

       public void InitialiseWindow(SituationWindow w)
       {
            situationWindow = w;
           w.Initialise(this);
       }
    

       public void OpenSituation()
       {
           // situationWindow.transform.position = (situationToken as DraggableToken).transform.position;
            situationWindow.Show();

           situationToken.OpenToken();

            if (SituationStateMachine!=null)
                situationWindow.DisplayOngoing();
            else
                situationWindow.DisplayStarting();

        }


        public void CloseSituation()
        {
           situationToken.CloseToken();
            situationWindow.Hide();
        }


       public void UpdateSituationDisplay()
        {
            var allAspects = GetAspectsAvailableToSituation();
            situationWindow.DisplayAspects(allAspects);

            string prediction = "";
            if(SituationStateMachine!=null)
            { 
            RecipeConductor rc = new RecipeConductor(Registry.Compendium, allAspects,
            new Dice());
            prediction= SituationStateMachine.GetPrediction(rc);
            situationWindow.DisplaySituation(SituationStateMachine.GetTitle(),SituationStateMachine.GetDescription(),prediction);
            }
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


       public void DisplayStartingRecipe()
        {
            AspectsDictionary startingAspects = situationWindow.GetAspectsFromSlottedElements();

            var r = Registry.Compendium.GetFirstRecipeForAspectsWithVerb(startingAspects, situationToken.Id);

            situationWindow.DisplayRecipe(r);
        }


        public HeartbeatResponse ExecuteHeartbeat(float interval)
        {
            HeartbeatResponse response=new HeartbeatResponse();
            if (SituationStateMachine != null)
            {
                RecipeConductor rc = new RecipeConductor(Registry.Compendium,
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

            RecipeConductor rc = new RecipeConductor(Registry.Compendium, situationToken.GetAspectsFromStoredElements(),
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

            situationToken.SituationEnding();


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
            var recipe = Registry.Compendium.GetFirstRecipeForAspectsWithVerb(aspects, situationToken.Id);
            if (recipe != null)
            {
                situationToken.StoreStacks(situationWindow.GetStacksInStartingSlots());

                BeginSituation(recipe);
                situationWindow.DisplayOngoing();
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
                OpenSituation();

       }
   }
}
