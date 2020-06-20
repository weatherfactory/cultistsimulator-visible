using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
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
    public class Ending:IEntityWithId
    {
        private string _id;

        [FucineId]
        public string Id
        {
            get => _id;
        }

        public void SetId(string id)
        {
            _id = id;
        }

        [FucineValue("")]
        public string Label { get; set; }

        [FucineValue("")]
        public string Description { get; set; }

        [FucineValue("")]
        public string Image { get; set; }

        [FucineValue((int)EndingFlavour.Melancholy)]
        public EndingFlavour EndingFlavour { get; set; }

        [FucineValue("")]
        public string Anim { get; set; }

        [FucineValue("")]
        public string AchievementId { get; set; }

        public Ending()
        {

        }

        public Ending(string id, string label, string description,string image,EndingFlavour endingFlavour, string anim,string achievementId)
        {
            _id = id;
            Label = label;
            Description = description;
            Image = image;
            EndingFlavour = endingFlavour;
            Anim = anim;
            AchievementId = achievementId;
        }

        

        public static Ending DefaultEnding()
        {
            Ending defaultEnding = new Ending
            {
                _id = "default",
                Label = "IT'S ALWAYS TOO LATE, EVENTUALLY",
                Description = "'... but until then, it's not.'",
                Image = "suninrags",
                EndingFlavour = EndingFlavour.Melancholy,
                Anim = "DramaticLight",
                AchievementId = null
            };


            return defaultEnding;

        }

        public void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {

        }
    }
}
