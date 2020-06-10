using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.TabletopUi.SlotsContainers;
using Noon;

namespace Assets.Core.Entities
{

    /// <summary>
    /// A specification for an effect available to the player after a game completes, which determines the starting situation of the next character.
    /// </summary>
    public class Legacy:IEntity
    {
        [FucineString]
        public string Id { get; set; }

        /// <summary>
        /// Title that displays at game end
        /// </summary>
        [FucineString]
        public string Label { get; set; }

        /// <summary>
        /// Detail that displays at game end
        /// </summary>
        [FucineString]
        public string Description { get; set; }

        /// <summary>
        /// Displays after game start
        /// </summary>
        [FucineString]
        public string StartDescription { get; set;}
        
        [FucineString]
        public string Image { get; set; }
        
        [FucineString("")]
        public string FromEnding { get; set; }

        [FucineBool(false)]
        public bool AvailableWithoutEndingMatch { get; set; }

        [FucineAspectsDictionary]
        public IAspectsDictionary Effects { get; set; }
        
        [FucineListString]
        public List<string> ExcludesOnEnding { get; set; }
       
        [FucineListString]
        public List<string> StatusBarElements { get; set; }

        [FucineString]
        public string StartingVerbId { get; set; }

        public Legacy()
        {

        }

        //public Legacy(string id, string label, string description, string startdescription, string image,
        //    string fromEnding, bool availableWithoutEndingMatch, List<string> excludesOnEnding, List<string> statusBarElements, string startingVerbId)
        //{
        //    Id = id;
        //    Label = label;
        //    Description = description;
        //    StartDescription = startdescription;
        //    Image = image;
        //    Effects = new AspectsDictionary();
        //    FromEnding = fromEnding;
        //    AvailableWithoutEndingMatch = availableWithoutEndingMatch;
        //    StatusBarElements = statusBarElements;
        //    ExcludesOnEnding = excludesOnEnding;

        //    if (string.IsNullOrEmpty(startingVerbId))
        //        StartingVerbId = NoonConstants.DEFAULT_STARTING_VERB_ID;
        //    else
        //        StartingVerbId =
        //            startingVerbId;

        //}
    }
}
