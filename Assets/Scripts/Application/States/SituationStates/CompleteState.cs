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
        public override StateEnum RehydrationValue => StateEnum.Complete;

        public override void Enter(Situation situation)
        {
            var createOutputSphereCommand = new PopulateDominionCommand(DominionEnum.Output,new SphereSpec(typeof(OutputSphere),nameof(OutputSphere)));
            situation.CommandQueue.AddCommand(createOutputSphereCommand);

            var migrateToOutputCommand=new FlushTokensToCategoryCommand(SphereCategory.SituationStorage,SphereCategory.Output,StateEnum.Complete);
            migrateToOutputCommand.RetireFromSpheres = true;
            situation.CommandQueue.AddCommand(migrateToOutputCommand);

            var attemptAspectInductionsCommand=new AttemptAspectInductionCommand(SphereCategory.Output,StateEnum.Complete);
            situation.CommandQueue.AddCommand(attemptAspectInductionsCommand);

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