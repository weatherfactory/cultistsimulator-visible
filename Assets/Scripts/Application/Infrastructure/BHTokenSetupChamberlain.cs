using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Infrastructure
{
    public class BHTokenSetupChamberlain: AbstractTokenSetupChamberlain
    {
        public override List<TokenCreationCommand> GetTokenCreationCommandsToEnactLegacy(Legacy forLegacy)
        {
            
            FucinePath librarySpherePath = Watchman.Get<HornedAxe>().GetDefaultSpherePath();

            var commands = new List<TokenCreationCommand>();

           // SituationCreationCommand startingSituation = new SituationCreationCommand(forLegacy.StartingVerbId);
           // var startingDestinationForVerb = new TokenLocation(startingTokenDistributionStrategy.GetNextTokenPositionAndIncrementCount(), tabletopSpherePath);

       //     TokenCreationCommand startingTokenCommand = new TokenCreationCommand(startingSituation, startingTokenDistributionStrategy.AboveBoardStartingLocation()).WithDestination(startingDestinationForVerb, startingTokenDistributionStrategy.GetPlacementDelay());
        //    commands.Add(startingTokenCommand);

            AspectsDictionary startingElements = new AspectsDictionary();
            startingElements.CombineAspects(forLegacy.Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.

            foreach (var e in startingElements)
            {
                var elementStackCreationCommand = new ElementStackCreationCommand(e.Key, e.Value);
                
                TokenCreationCommand startingStackCommand = new TokenCreationCommand(elementStackCreationCommand, TokenLocation.Default(librarySpherePath));
                commands.Add(startingStackCommand);
            }

            return commands;
        }
    }
}
