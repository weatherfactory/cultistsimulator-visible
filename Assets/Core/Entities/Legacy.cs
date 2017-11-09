﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    //TODO: rename Martin's UI class once he's done in there
    /// <summary>
    /// A specification for an effect available to the player after a game completes, which determines the starting situation of the next character.
    /// </summary>
    public class Legacy
    {
        public string Id { get; set; }

        //title that displays at game end
        public string Label { get; set; }
        
        //detail thatdisplays at game end
        public string Description { get; set; }

        //displays after game start
        public string StartDescription { get; set; }
        public string Image { get; set; }
        public IAspectsDictionary Effects;


        public Legacy(string id, string label, string description, string startdescription, string image)
        {
            Id = id;
            Label = label;
            Description = description;
            StartDescription = startdescription;
            Image = image;
            Effects = new AspectsDictionary();
        }
    }
}
