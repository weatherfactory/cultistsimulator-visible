using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Infrastructure
{
    public class BHTokenSetupChamberlain: AbstractTokenSetupChamberlain
    {
        public override List<TokenCreationCommand> GetTokenCreationCommandsToEnactLegacy(Legacy forLegacy)
        {
         //This is used to populate the root command. Use sphere paths not spheres!   
         FucinePath startingRoomSpherePath = new FucinePath("*/solarium1");

      
            var commands = new List<TokenCreationCommand>();

           // SituationCreationCommand startingSituation = new SituationCreationCommand(forLegacy.StartingVerbId);
           // var startingDestinationForVerb = new TokenLocation(startingTokenDistributionStrategy.GetNextTokenPositionAndIncrementCount(), tabletopSpherePath);

       //     TokenCreationCommand startingTokenCommand = new TokenCreationCommand(startingSituation, startingTokenDistributionStrategy.AboveBoardStartingLocation()).WithDestination(startingDestinationForVerb, startingTokenDistributionStrategy.GetPlacementDelay());
        //    commands.Add(startingTokenCommand);


        foreach (var effect in forLegacy.Effects)
        {
            var effectPath = new FucinePath(effect.Key);
            string elementToCreate = effectPath.GetEndingPathPart().TrimTokenPrefix();
            FucinePath createAtSpherePath= effectPath.GetSpherePath();
            var elementStackCreationCommand = new ElementStackCreationCommand(elementToCreate, effect.Value);
            TokenCreationCommand startingTokenCreationCommand = new TokenCreationCommand(elementStackCreationCommand, TokenLocation.Default(createAtSpherePath));
            
            commands.Add(startingTokenCreationCommand);
            }


            return commands;
        }
    }
}
