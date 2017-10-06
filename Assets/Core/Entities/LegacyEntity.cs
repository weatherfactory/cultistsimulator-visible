using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    //TODO: rename Martin's UI class once he's done in there
    /// <summary>
    /// A specification for an effect available to the player after a game completes, which determines the starting situation of the next character.
    /// </summary>
    public class LegacyEntity
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public IAspectsDictionary ElementEffects;

        public LegacyEntity()
        {
            ElementEffects=new AspectsDictionary();
        }
    }
}
