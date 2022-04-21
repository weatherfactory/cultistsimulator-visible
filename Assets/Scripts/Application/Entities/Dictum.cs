using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;

namespace SecretHistories.Entities
{
    [FucineImportable("dicta")]
    public class Dictum: AbstractEntity<Dictum>
    {

        [FucineValue] public string WorldSphereType { get; set; }
        [FucinePathValue] public FucinePath DefaultWorldSpherePath { get; set; }
        [FucineDict]
        public Dictionary<string, string> AlternativeDefaultWorldSpherePaths { get; set; }


        [FucineValue]
        public string LogoScene { get; set; }

        [FucineValue]
        public string QuoteScene { get; set; }

        [FucineValue]
        public string MenuScene { get; set; }

        [FucineValue]
        public string PlayfieldScene { get; set; }

        [FucineValue]
        public string GameOverScene { get; set; }

        [FucineValue]
        public string NewGameScene { get; set; }

        //We can't put the UhO scene in Dictum, because if the dictum JSON fails to load, we'll get stuck in a loop!

        [FucineValue]
        public string NoteElementId { get; set; }

        [FucineValue]
        public float DefaultTravelDuration { get; set; }

        [FucineValue]
        public float DefaultQuickTravelDuration { get; set; }
        public Dictum(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
            
        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            //
        }
    }
}
