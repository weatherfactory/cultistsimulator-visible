using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;

namespace Assets.TabletopUi
{
   public  class SituationController:ISituationSubscriber
   {
       public readonly SituationToken situationToken;
       private SituationWindow situationWindow;

       public SituationWindow LinkedSituationWindow { get { return situationWindow;} }
       public Situation situation;


       public SituationController(SituationToken t, SituationWindow w)
       {
           situationToken = t;
           situationWindow = w;
       }



       public void DisplayCurrentRecipe()
       {
            AspectsDictionary allAspects=new AspectsDictionary();
            
       }

       public void DisplayRecipeForAspects(AspectsDictionary aspects)
       {
           situationWindow.DisplayRecipeForAspects(aspects);
       }

       public void PopulateAndShowWindow()
       {
            situationWindow.PopulateAndShow(this);
        }


        public void ExecuteHeartbeat(float interval)
        {
            if (situation != null)
            {
                RecipeConductor rc = new RecipeConductor(Registry.Compendium,
                    situationToken.GetSituationStacksGateway().GetTotalAspects(), new Dice());
                situation.Continue(rc, interval);
            }
        }

        public string GetNextRecipeDescription()
        {
            RecipeConductor rc = new RecipeConductor(Registry.Compendium, situationToken.GetSituationStacksGateway().GetTotalAspects(),
                new Dice());
            return situation.GetPrediction(rc);
        }


        public void BeginSituation(Recipe r)
        {
            situation = new Situation(r);
            situation.Subscribe(this);
        }

        public void SituationBeginning(Situation s)
       {

           situationToken.InitialiseSlotContainerForSituation(s);
            situationToken.SetTimerVisibility(true);
            SituationContinues(s);
            situationWindow.DisplaySituation(s.GetTitle(),s.GetDescription(),GetNextRecipeDescription());
       }

        public void SituationContinues(Situation s)
        {
            situationToken.DisplayTimeRemaining(s.Warmup, s.TimeRemaining);
        }



        public void SituationExecutingRecipe(IEffectCommand command)
       {
            foreach (var kvp in command.GetElementChanges())
            {
               situationToken.GetSituationStacksGateway().ModifyElementQuantity(kvp.Key, kvp.Value);
            }
          situationToken.queuedNotifications.Add(new Notification(command.Title, command.Description));
        }

       public void SituationExtinct()
       {
            IElementStacksGateway storedStacksGateway = situationToken.GetSituationStacksGateway();

            //currently just retrieving everything
            var stacksToRetrieve = storedStacksGateway.GetStacks();

            situationWindow.GetStacksGatewayForOutput().AcceptStacks(stacksToRetrieve);

            situationToken.SetTimerVisibility(false);
            situation = null;
        }
   }
}
