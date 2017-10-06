using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    public class Ending
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public Ending(string title, string description)
        {
            Title = title;
            Description = description;
        }
    }
}
