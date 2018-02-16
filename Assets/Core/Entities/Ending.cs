using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    public enum EndingFlavour
    {
        Grand=1,
        Melancholy=2
    }

    public class Ending
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageId { get; set; }
        public EndingFlavour EndingFlavour { get; set; }
        public string Anim { get; set; }

        public Ending(string id, string title, string description,string imageId,EndingFlavour endingFlavour, string anim)
        {
            Id = id;
            Title = title;
            Description = description;
            ImageId = imageId;
            EndingFlavour = endingFlavour;
            Anim = anim;
        }

        public static Ending DefaultEnding()
        {
        return new Ending("default", "IT IS FINISHED","This one is done.", "suninrags", EndingFlavour.Melancholy, "DramaticLight");
        }
    }
}
