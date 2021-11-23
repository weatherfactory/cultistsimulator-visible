using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SecretHistories.Assets.Scripts.Application.Tokens;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.Core;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;


namespace SecretHistories.Entities
{

    /// <summary>
    /// A specification for an effect available to the player after a game completes, which determines the starting situation of the next character.
    /// </summary>
    [FucineImportable("legacies")]
    public class Legacy: AbstractEntity<Legacy>, IEntityWithId
    {


        /// <summary>
        /// Title that displays at game end
        /// </summary>
        [FucineValue(DefaultValue = "", Localise = true)]
        public string Label { get; set; }

        /// <summary>
        /// For legacies that we want to group together, like the various Exile starts
        /// </summary>
        [FucineValue(DefaultValue = "")]
        public string Family { get; set; }

        /// <summary>
        /// Detail that displays at game end
        /// </summary>
        [FucineValue(DefaultValue = "", Localise = true)]
        public string Description { get; set; }

        /// <summary>
        /// Displays after game start
        /// </summary>
        [FucineValue(DefaultValue = "", Localise = true)]
        public string StartDescription { get; set;}

        [FucineValue("")]
        public string Image { get; set; }

        [FucineValue("")]
        public string TableCoverImage { get; set; }

        [FucineValue("")]
        public string TableSurfaceImage { get; set; }

        [FucineValue("")]
        public string TableEdgeImage { get; set; }

        [FucineValue("")]
        public string FromEnding { get; set; }

        [FucineValue(false)]
        public bool AvailableWithoutEndingMatch { get; set; }


        [FucineValue(false)]
        public bool NewStart { get; set; }

        [FucineAspects(ValidateAsElementId = true)]
        public AspectsDictionary Effects { get; set; }

        [FucineList]
        public List<string> ExcludesOnEnding { get; set; }

        [FucineList]
        public List<string> StatusBarElements { get; set; }

        [FucineValue(".")]
        public string StartingVerbId { get; set; }

        public virtual bool IsValid()
        {
            return true;
        }

        public Legacy(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        public Legacy()
        {

        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            
        }

        public List<TokenCreationCommand> GetTokenCreationCommandsToEnactLegacy()
        {
            var startingTokenDistributionStrategy=new CSClassicStartingTokenDistributionStrategy();

            FucinePath tabletopSpherePath = Watchman.Get<HornedAxe>().GetDefaultSpherePath();

            var commands = new List<TokenCreationCommand>();

            SituationCreationCommand startingSituation = new SituationCreationCommand(StartingVerbId);
            var startingDestinationForVerb = new TokenLocation(startingTokenDistributionStrategy.GetNextTokenPositionAndIncrementCount(), tabletopSpherePath);

            TokenCreationCommand startingTokenCommand = new TokenCreationCommand(startingSituation, startingTokenDistributionStrategy.AboveBoardStartingLocation()).WithDestination(startingDestinationForVerb, startingTokenDistributionStrategy.GetPlacementDelay());
            commands.Add(startingTokenCommand);

            AspectsDictionary startingElements = new AspectsDictionary();
            startingElements.CombineAspects(Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.

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
            var elementDropzoneTokenCreationCommand = new TokenCreationCommand(elementDropzoneCreationCommand, startingTokenDistributionStrategy.BelowBoardStartingLocation()).WithDestination(elementDropzoneLocation,startingTokenDistributionStrategy.GetPlacementDelay());
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
                var startingPosition=new Vector3(x,y,0);
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
