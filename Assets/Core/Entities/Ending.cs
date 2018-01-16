using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    public class Ending
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageId { get; set; }
        public string Anim { get; set; }

        public Ending(string id, string title, string description,string imageId,string anim)
        {
            Id = id;
            Title = title;
            Description = description;
            ImageId = imageId;
            Anim = anim;
        }
    }
}
