using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

namespace Assets.Logic
{
    public class LegacySelector
    {
        private readonly ICompendium _compendium;

        public List<LegacyEntity> DetermineLegacies(Ending ending, List<IElementStack> stacksAtEnd)
        {
            //sample legacies
            var l1 = new LegacyEntity()
            {
                Id = "1",
                Label = "Inheritor",
                Description = "You'll receive a mysterious package"
            };
            var l2=new LegacyEntity()
            {
                Id = "2",
                Label = "Doctor",
                Description = "You've made copious notes on this curious case."
            }; ;
            var l3=new LegacyEntity()
            {
                Id = "3",
                Label = "Haunted",
                Description = "Your dreams are haunted by visions of one long gone."
            };

            var legacies = new List<LegacyEntity> {l1, l2, l3};

            return legacies;
        }

        public LegacySelector(ICompendium compendium)
        {
            _compendium = compendium;
        }
    }
}