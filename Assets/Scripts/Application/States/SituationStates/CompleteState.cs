using System;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Constants;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.States
{
    public class CompleteState : SituationState
    {
        public override bool AllowDuplicateVerbIfVerbSpontaneous => true;
        public override StateEnum Identifier => StateEnum.Complete;
        public override bool UpdatePredictionDynamically => false;

        public override void Enter(Situation situation)
        {
            var createOutputSphereCommand = new PopulateDominionCommand(SituationDominionEnum.Output.ToString(),new SphereSpec(typeof(OutputSphere),nameof(OutputSphere)));
            situation.AddCommand(createOutputSphereCommand);

            var migrateToOutputCommand=new FlushTokensToCategoryCommand(SphereCategory.SituationStorage,SphereCategory.Output,StateEnum.Complete);
            situation.AddCommand(migrateToOutputCommand);

            var attemptAspectInductionsCommand=new AttemptAspectInductionCommand(SphereCategory.Output,StateEnum.Complete);
            situation.AddCommand(attemptAspectInductionsCommand);


            //remove verb, recipe and storage slots here.
            var clearVerbThresholdsCommand =new ClearDominionCommand(SituationDominionEnum.VerbThresholds.ToString(),SphereRetirementType.Graceful);
            situation.AddCommand(clearVerbThresholdsCommand);
            var clearRecipeThresholdsCommand = new ClearDominionCommand(SituationDominionEnum.RecipeThresholds.ToString(), SphereRetirementType.Graceful);
            situation.AddCommand(clearRecipeThresholdsCommand);
            var clearStorageThresholdsCommand = new ClearDominionCommand(SituationDominionEnum.Storage.ToString(), SphereRetirementType.Graceful);
            situation.AddCommand(clearStorageThresholdsCommand);



            SoundManager.PlaySfx("SituationComplete"); //this could run through that Echo obj

        }

        public override void Exit(Situation situation)
        {
       
        }

        public override bool IsActiveInThisState(Sphere s)
        {
            if (s.SphereCategory == SphereCategory.Output)
                return true;

            return false;
        }


        public override bool IsValidPredictionForState(Recipe recipeToCheck, Situation s)
        {
            return false;
        }

        public override void Continue(Situation situation)
        {
            

        }
    }
}