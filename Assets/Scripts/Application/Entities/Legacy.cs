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
            FucinePath tabletopSpherePath = Watchman.Get<HornedAxe>().GetDefaultWorldSpherePath();

            var commands = new List<TokenCreationCommand>();

            SituationCreationCommand startingSituation = new SituationCreationCommand(StartingVerbId, new FucinePath(StartingVerbId));
            TokenCreationCommand startingTokenCommand = new TokenCreationCommand(startingSituation, TokenLocation.Default(tabletopSpherePath));
            commands.Add(startingTokenCommand);

            AspectsDictionary startingElements = new AspectsDictionary();
            startingElements.CombineAspects(Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.

            foreach (var e in startingElements)
            {
                var elementStackCreationCommand = new ElementStackCreationCommand(e.Key, e.Value);
                TokenCreationCommand startingStackCommand = new TokenCreationCommand(elementStackCreationCommand, TokenLocation.Default(tabletopSpherePath));
                commands.Add(startingStackCommand);
            }

            return commands;
        }
    }
}
