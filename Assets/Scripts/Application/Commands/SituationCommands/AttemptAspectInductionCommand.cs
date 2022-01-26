using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;

namespace SecretHistories.Commands.SituationCommands
{
   public class AttemptAspectInductionCommand: ISituationCommand
    {

        private readonly SphereCategory _forSphereCategory;

        private readonly List<StateEnum> _statesCommandIsValidFor = new List<StateEnum>();

        public bool IsValidForState(StateEnum forState)
        {
            return (_statesCommandIsValidFor.Contains(forState));
        }

        public bool IsObsoleteInState(StateEnum forState)
        {
            return false;
        }

        public AttemptAspectInductionCommand(SphereCategory forSphereCategory, StateEnum onState)
        {
            _forSphereCategory = forSphereCategory;
            _statesCommandIsValidFor.Add(onState);
            }

        public bool Execute(Situation situation)
        {
            //If any elements in the output, or in the situation itself, have inductions, test whether to start a new recipe

            var inducingAspects = new AspectsDictionary();

            //shrouded cards don't trigger inductions. This is because we don't generally want to trigger an induction
            //for something that has JUST BEEN CREATED. This started out as a hack, but now we've moved from 'face-down'
            //to 'shrouded' it feels more suitable.

            var spheresToGetFrom = situation.GetSpheresByCategory(_forSphereCategory);
            List<Token> tokens=new List<Token>();

            foreach (var s in spheresToGetFrom)
            {
                tokens.AddRange(s.GetElementTokens());
            }



            foreach (var t in tokens)
            {
                if (!t.Shrouded())
                    inducingAspects.CombineAspects(t.GetAspects(true));
            }


            inducingAspects.CombineAspects(situation.Recipe.Aspects);


            foreach (var a in inducingAspects)
            {
                var aspectElement = Watchman.Get<Compendium>().GetEntityById<Element>(a.Key);

                if (aspectElement != null)
                    PerformAspectInduction(aspectElement,situation);
                else
                    NoonUtility.Log("unknown aspect " + a + " in output");
            }

            return true;
        }


        void PerformAspectInduction(Element aspectElement, Situation situation)
        {
            foreach (var induction in aspectElement.Induces)
            {
                var d = Watchman.Get<IDice>();

                if (d.Rolld100() <= induction.Chance)
                    CreateRecipeFromInduction(Watchman.Get<Compendium>().GetEntityById<Recipe>(induction.Id), aspectElement.Id,situation);
            }
        }

        void CreateRecipeFromInduction(Recipe inducedRecipe, string aspectID, Situation situation) //REFACTOR: yeah this *definitely* should be through subscription!
        {
            if (inducedRecipe == null)
            {
                NoonUtility.Log("unknown recipe " + inducedRecipe + " in induction for " + aspectID);
                return;
            }


            SituationCreationCommand inducedSituationCreationCommand =
                new SituationCreationCommand(inducedRecipe.ActionId).WithRecipeAboutToActivate(inducedRecipe.Id);

            var spawnNewTokenCommand = new SpawnNewTokenFromThisOneCommand(inducedSituationCreationCommand, new Context(Context.ActionSource.JustSpawned));
            situation.Token.ExecuteTokenEffectCommand(spawnNewTokenCommand);

        }
    }
}
