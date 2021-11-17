using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
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
            var verbTokenLocation=new TokenLocation(startingTokenDistributionStrategy.GetNextTokenPositionAndIncrementCount(),tabletopSpherePath);

            TokenCreationCommand startingTokenCommand = new TokenCreationCommand(startingSituation, verbTokenLocation);
            commands.Add(startingTokenCommand);

            AspectsDictionary startingElements = new AspectsDictionary();
            startingElements.CombineAspects(Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.

            foreach (var e in startingElements)
            {
                var elementStackCreationCommand = new ElementStackCreationCommand(e.Key, e.Value);
                var startingLocationForToken = new TokenLocation(startingTokenDistributionStrategy.GetNextTokenPositionAndIncrementCount(), tabletopSpherePath);
                TokenCreationCommand startingStackCommand = new TokenCreationCommand(elementStackCreationCommand, startingLocationForToken);
                commands.Add(startingStackCommand);
            }


            startingTokenDistributionStrategy.NextRow();

            var dropzoneLocation = new TokenLocation(startingTokenDistributionStrategy.GetNextTokenPositionAndIncrementCount(), tabletopSpherePath);
            var dropzoneCreationCommand = new DropzoneCreationCommand();
            var bubbleSphereSpec = new SphereSpec(typeof(BubbleSphere), "classicdropzonebubble");
            dropzoneCreationCommand.Dominions.Add(new PopulateDominionCommand(SituationDominionEnum.Unknown.ToString(), bubbleSphereSpec));
             var dropzoneTokenCreationCommand = new TokenCreationCommand(dropzoneCreationCommand, dropzoneLocation);
            commands.Add(dropzoneTokenCreationCommand);

            return commands;
        }

        class CSClassicStartingTokenDistributionStrategy
        {
            public int tokenCount { get; private set; }
            public int rowCount { get; private set; }
            private const int STARTINGX = -300;
            private const int XGAP = 200;
            private const int STARTINGY = 200;
            private const int YGAP = 200;

            public Vector3 GetNextTokenPositionAndIncrementCount()
            {
                int x = STARTINGX + (tokenCount * XGAP);
                int y = STARTINGY - (YGAP * rowCount); //nb minus: next row is below previous one
                var startingPosition=new Vector3(x,y,0);
                tokenCount++;
                return startingPosition;
            }

            public void NextRow()
            {
                tokenCount = 0;
                rowCount++;
            }
        }
    }
}
