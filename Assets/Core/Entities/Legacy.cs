using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.TabletopUi.SlotsContainers;
using Noon;

namespace Assets.Core.Entities
{

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
        public string FromEnding { get; set; }
        public bool AvailableWithoutEndingMatch { get; set; }
        public IAspectsDictionary Effects;
        public List<string> ExcludesOnEnding;
        public List<string> StatusBarElements;
public string StartingVerbId { get; private set; }

        public Legacy(string id, string label, string description, string startdescription, string image,
            string fromEnding, bool availableWithoutEndingMatch, List<string> excludesOnEnding, List<string> statusBarElements, string startingVerbId)
        {
            Id = id;
            Label = label;
            Description = description;
            StartDescription = startdescription;
            Image = image;
            Effects = new AspectsDictionary();
            FromEnding = fromEnding;
            AvailableWithoutEndingMatch = availableWithoutEndingMatch;
            StatusBarElements = statusBarElements;
            ExcludesOnEnding = excludesOnEnding;

            if (string.IsNullOrEmpty(startingVerbId))
                StartingVerbId = NoonConstants.DEFAULT_STARTING_VERB_ID;
            else
                StartingVerbId =
                    startingVerbId;

        }
    }
}
