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

    public class Ending:IEntity
    {
        public string Id { get; set; }
        public void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {
            
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageId { get; set; }
        public EndingFlavour EndingFlavour { get; set; }
        public string Anim { get; set; }
        public string AchievementId { get; set; }


        public Ending(string id, string title, string description,string imageId,EndingFlavour endingFlavour, string anim,string achievementId)
        {
            Id = id;
            Title = title;
            Description = description;
            ImageId = imageId;
            EndingFlavour = endingFlavour;
            Anim = anim;
            AchievementId = achievementId;
        }

        

        public static Ending DefaultEnding()
        {
        return new Ending("default", "IT IS FINISHED","This one is done.", "suninrags", EndingFlavour.Melancholy, "DramaticLight",null);
        }
    }
}
