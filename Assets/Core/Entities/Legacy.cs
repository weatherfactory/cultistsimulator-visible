using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using Assets.TabletopUi.SlotsContainers;
using Noon;

namespace Assets.Core.Entities
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
        public string FromEnding { get; set; }

        [FucineValue(false)]
        public bool AvailableWithoutEndingMatch { get; set; }


        [FucineValue(false)]
        public bool NewStart { get; set; }

        [FucineAspects(ValidateAsElementId = true)]
        public IAspectsDictionary Effects { get; set; }

        [FucineList]
        public List<string> ExcludesOnEnding { get; set; }

        [FucineList]
        public List<string> StatusBarElements { get; set; }

        [FucineValue(".")]
        public string StartingVerbId { get; set; }

        public Legacy(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        public Legacy()
        {

        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
            
        }
    }
}
