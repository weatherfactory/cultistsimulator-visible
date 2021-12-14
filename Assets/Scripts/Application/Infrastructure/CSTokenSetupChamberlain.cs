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
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Infrastructure
{
    public class CSTokenSetupChamberlain: AbstractTokenSetupChamberlain
    {
        public override List<TokenCreationCommand> GetTokenCreationCommandsToEnactLegacy(Legacy forLegacy)
        {
            var startingTokenDistributionStrategy = new CSClassicStartingTokenDistributionStrategy();

            FucinePath tabletopSpherePath = Watchman.Get<HornedAxe>().GetDefaultSpherePath();

            var commands = new List<TokenCreationCommand>();

            SituationCreationCommand startingSituation = new SituationCreationCommand(forLegacy.StartingVerbId);
            var startingDestinationForVerb = new TokenLocation(startingTokenDistributionStrategy.GetNextTokenPositionAndIncrementCount(), tabletopSpherePath);

            TokenCreationCommand startingTokenCommand = new TokenCreationCommand(startingSituation, startingTokenDistributionStrategy.AboveBoardStartingLocation()).WithDestination(startingDestinationForVerb, startingTokenDistributionStrategy.GetPlacementDelay());
            commands.Add(startingTokenCommand);

            AspectsDictionary startingElements = new AspectsDictionary();
            startingElements.CombineAspects(forLegacy.Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.

            foreach (var e in startingElements)
            {
                var elementStackCreationCommand = new ElementStackCreationCommand(e.Key, e.Value);
                var startingDestinationForToken = new TokenLocation(startingTokenDistributionStrategy.GetNextTokenPositionAndIncrementCount(), tabletopSpherePath);
                TokenCreationCommand startingStackCommand = new TokenCreationCommand(elementStackCreationCommand, startingTokenDistributionStrategy.AboveBoardStartingLocation()).WithDestination(startingDestinationForToken, startingTokenDistributionStrategy.GetPlacementDelay());
                commands.Add(startingStackCommand);
            }

            startingTokenDistributionStrategy.NextRow();

            var elementDropzoneLocation = new TokenLocation(startingTokenDistributionStrategy.GetNextTokenPositionAndIncrementCount(), tabletopSpherePath);
            var elementDropzoneCreationCommand = new DropzoneCreationCommand(nameof(ElementStack).ToString());
            var elementDropzoneTokenCreationCommand = new TokenCreationCommand(elementDropzoneCreationCommand, startingTokenDistributionStrategy.BelowBoardStartingLocation()).WithDestination(elementDropzoneLocation, startingTokenDistributionStrategy.GetPlacementDelay());
            commands.Add(elementDropzoneTokenCreationCommand);

            startingTokenDistributionStrategy.NextRow();

            var situationDropzoneLocation = new TokenLocation(startingTokenDistributionStrategy.GetNextTokenPositionAndIncrementCount(), tabletopSpherePath);
            var situationDropzoneCreationCommand = new DropzoneCreationCommand(nameof(Situation).ToString());
            var situationDropzoneTokenCreationCommand = new TokenCreationCommand(situationDropzoneCreationCommand, startingTokenDistributionStrategy.BelowBoardStartingLocation()).WithDestination(situationDropzoneLocation, startingTokenDistributionStrategy.GetPlacementDelay());
            commands.Add(situationDropzoneTokenCreationCommand);

            return commands;

        }

        class CSClassicStartingTokenDistributionStrategy
        {
            public int tokenCountOnThisRow { get; private set; }
            public int rowCount { get; private set; }
            private const int STARTINGX = -300;
            private const int XGAP = 200;
            private const int STARTINGY = 200;
            private const int YGAP = 200;

            public Vector3 GetNextTokenPositionAndIncrementCount()
            {
                int x = STARTINGX + (tokenCountOnThisRow * XGAP);
                int y = STARTINGY - (YGAP * rowCount); //nb minus: next row is below previous one
                var startingPosition = new Vector3(x, y, 0);
                tokenCountOnThisRow++;
                return startingPosition;
            }

            public float GetPlacementDelay()
            {
                var quickDuration = Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultQuickTravelDuration;
                var longDuration = Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultTravelDuration;
                var extraDelayForEachTokenOnRow = (tokenCountOnThisRow + 1) * quickDuration;
                var extraDelayForEachRow = rowCount * longDuration;
                return extraDelayForEachRow + extraDelayForEachTokenOnRow;
            }

            public void NextRow()
            {
                tokenCountOnThisRow = 0;
                rowCount++;
            }

            public TokenLocation AboveBoardStartingLocation()
            {
                return new TokenLocation(0, 2000, 0, Watchman.Get<HornedAxe>().GetDefaultSpherePath());
            }

            public TokenLocation BelowBoardStartingLocation()
            {
                return new TokenLocation(0, -2000, 0, Watchman.Get<HornedAxe>().GetDefaultSpherePath());
            }
        }
    }

}

