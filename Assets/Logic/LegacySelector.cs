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
            //TODO: filter logic
            return _compendium.GetAllLegacies();
        }

        public LegacySelector(ICompendium compendium)
        {
            _compendium = compendium;
        }
    }
}