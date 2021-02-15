using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Spheres.SphereSpecIdentifierStrategies;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Application.Meta
{
   public class SituationsMalleary: MonoBehaviour
   {
       [SerializeField] private AutoCompletingInput input;
       [SerializeField] private ThresholdSphere _situationDrydock;

       public void Awake()
       {
           var sphereSphec = new SphereSpec(new PrimaryThresholdSphereSpecIdentifierStrategy());
           sphereSphec.SetId("situationsmalleary");
           sphereSphec.Label = "Malleary: Situations";
           sphereSphec.AllowAnyToken = true;
     var spherePath=new SpherePath(SituationPath.Root(),sphereSphec.Id);
           
           _situationDrydock.SetUpWithSphereSpecAndPath(sphereSphec, spherePath);
       }


        public void CreateSituation()
       {
           string entityId = input.text;

           var compendium = Watchman.Get<Compendium>();
           var recipe = compendium.GetEntityById<Recipe>(entityId.Trim());
           SituationCreationCommand newSituationCommand;
           if (recipe.IsValid())
           {
                newSituationCommand = new SituationCreationCommand(recipe.ActionId, new SituationPath(recipe.ActionId), StateEnum.Ongoing).WithRecipeId(recipe.Id);
                //assuming we want the whole lifetime of the recipe
                  newSituationCommand.TimeRemaining = recipe.Warmup;
           }
           else
           {
               //if not a recipe, could it be a verb? create it, unstarted.
               var verb = compendium.GetEntityById<Verb>(entityId);
               if (verb.IsValid())
                   newSituationCommand =
                       new SituationCreationCommand(verb.Id, new SituationPath(verb.Id), StateEnum.Unstarted);
               else
                   return;
           }
           var newTokenLocation = new TokenLocation(0f, 0f, 0f, _situationDrydock.GetPath());
           var newTokenCommand = new TokenCreationCommand(newSituationCommand, newTokenLocation);
           newTokenCommand.Execute(new Context(Context.ActionSource.Debug));

    
       }

        public void TimeForwards()
        {
            _situationDrydock.RequestTokensSpendTime(Heart.BEAT_INTERVAL_SECONDS * 50);
        }

        public void TimeBackwards()
        {
            _situationDrydock.RequestTokensSpendTime(Heart.BEAT_INTERVAL_SECONDS * -50);
        }

        public void AdvanceTimeToEndOfRecipe()
        {
            var situation = _situationDrydock.GetTokenInSlot().Payload;
            var timeRemaining = situation.GetTimeshadow().LifetimeRemaining;
            if(timeRemaining>0)
                _situationDrydock.RequestTokensSpendTime(timeRemaining);
        }

        public void AddNote()
        {
            var situation = _situationDrydock.GetTokenInSlot().Payload;
            string title = "!";
            string description = input.text;
            var addNoteCommand=new AddNoteCommand(title,description,new Context(Context.ActionSource.UI));

          situation.ExecuteTokenEffectCommand(addNoteCommand);
            }
    }
}
