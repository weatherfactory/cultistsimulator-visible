using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    public enum EndingFlavour
    {
        None=0,
        Grand=1,
        Melancholy=2,
        Pale=3,
        Vile=4
    }
    [FucineImportable("endings")]
    public class Ending: AbstractEntity<Ending>, IEntityWithId
    {

        [FucineValue(DefaultValue = "", Localise = true)]
        public string Label { get; set; }

        [FucineValue(DefaultValue = "", Localise = true)]
        public string Description { get; set; }

        [FucineValue("")]
        public string Comments { get; set; }

        [FucineValue("")]
        public string Image { get; set; }

        [FucineValue((int)EndingFlavour.Melancholy)]
        public EndingFlavour Flavour { get; set; }

        [FucineValue("")]
        public string Anim { get; set; }

        [FucineValue("")]
        public string Achievement { get; set; }

        public Ending(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        public Ending(string id, string label, string description,string image,EndingFlavour flavour, string anim,string achievement)
        {
            _id = id;
            Label = label;
            Description = description;
            Image = image;
            Flavour = flavour;
            Anim = anim;
            Achievement = achievement;
        }

        private Ending()
        {
        }


        public static Ending DefaultEnding()
        {
            Ending defaultEnding = new Ending
            {
                _id = "default",
                Label = "IT'S ALWAYS TOO LATE, EVENTUALLY",
                Description = "'... but until then, it's not.'",
                Image = "suninrags",
                Flavour = EndingFlavour.Melancholy,
                Anim = "DramaticLight",
                Achievement = null
            };


            return defaultEnding;

        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
            
        }
    }
}
