using System;
using SecretHistories.Assets.Scripts.Application.Commands.SituationCommands;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Constants;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;
using SSecretHistories.Entities;

namespace SecretHistories.States
{
    public class CompleteState : SituationState
    {
        public override bool AllowDuplicateVerbIfVerbSpontaneous => true;
        public override StateEnum RehydrationValue => StateEnum.Complete;

        public override void Enter(Situation situation)
        {
            var createOutputShereCommand = new PopulateDominionSpheresCommand(CommandCategory.Output, new OutputSphereSpec());
            situation.CommandQueue.AddCommand(createOutputShereCommand);

            var migrateToOutputCommand=new MigrateTokensInsideSituationCommand(SphereCategory.SituationStorage,SphereCategory.Output,CommandCategory.Output);
            situation.CommandQueue.AddCommand(migrateToOutputCommand);

            var attemptAspectInductionsCommand=new AttemptAspectInductionCommand(CommandCategory.Output,SphereCategory.Output);
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

        public override bool IsVisibleInThisState(Dominion dominion)
        {
            return dominion.VisibleFor(StateEnum.Complete);

        }

        public override bool IsValidPredictionForState(Recipe recipeToCheck, Situation s)
        {
            return false;
        }

        public override void Continue(Situation situation)
        {
            situation.CommandQueue.ExecuteCommandsFor(CommandCategory.Output, situation);

        }
    }
}