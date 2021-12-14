using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SecretHistories.Assets.Scripts.Application.Infrastructure;
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
    public class Legacy : AbstractEntity<Legacy>, IEntityWithId
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
        public string StartDescription { get; set; }

        [FucineValue("")] public string Image { get; set; }

        [FucineValue("")] public string TableCoverImage { get; set; }

        [FucineValue("")] public string TableSurfaceImage { get; set; }

        [FucineValue("")] public string TableEdgeImage { get; set; }

        [FucineValue("")] public string FromEnding { get; set; }

        [FucineValue(false)] public bool AvailableWithoutEndingMatch { get; set; }


        [FucineValue(false)] public bool NewStart { get; set; }

        [FucineAspects(ValidateAsElementId = true)]
        public AspectsDictionary Effects { get; set; }

        [FucineList] public List<string> ExcludesOnEnding { get; set; }

        [FucineList] public List<string> StatusBarElements { get; set; }

        [FucineValue(".")] public string StartingVerbId { get; set; }


        public AbstractTokenSetupChamberlain GetTokenSetupChamberlain(GameId gameId)
        {
            //this allows us to hook in different setup chamberlains for different legacies - or setup chamberlains with different values - later.

            if (gameId == GameId.CS)
                return new CSTokenSetupChamberlain();
            if (gameId == GameId.BH)
                return new BHTokenSetupChamberlain();
            else
                throw new NotImplementedException($"What chamberlain would we want for {gameId}?");

        }

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

    }
}
