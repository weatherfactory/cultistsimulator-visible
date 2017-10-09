using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

namespace Assets.Logic
{
    public class LegacySelector
    {
        private readonly ICompendium _compendium;

        public List<Legacy> DetermineLegacies(Ending ending, List<IElementStack> stacksAtEnd)
        {
            //sample legacies
            var l1 = new Legacy()
            {
                Id = "inheritor",
                Label = "Inheritor",
                Description = "You'll receive a mysterious package",
                ElementEffects = new AspectsDictionary() { { "shilling", 1 } }
            };
            var l2=new Legacy()
            {
                Id = "doctor",
                Label = "Doctor",
                Description = "You've made copious notes on this curious case.",
                ElementEffects = new AspectsDictionary() { { "shilling", 2} }
            }; 
            var l3=new Legacy()
            {
                Id = "haunted",
                Label = "Haunted",
                Description = "Your dreams are haunted by visions of one long gone.",
                ElementEffects = new AspectsDictionary() { { "shilling", 3 }, { "fragmentglory", 1 } },
                Image = "knock"
            };

            var legacies = new List<Legacy> {l1, l2, l3};

            return legacies;
        }

        public LegacySelector(ICompendium compendium)
        {
            _compendium = compendium;
        }
    }
}