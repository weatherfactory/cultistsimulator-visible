using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.Core.Interfaces
{
    public interface ISituationEffectCommand
    {
        string Title { get; }
        string Description { get; }
        Recipe Recipe { get; }
        Dictionary<string, int> GetElementChanges();
        bool AsNewSituation { get; }

        /// <summary>
        /// returns the deck to draw from if there is one, or null if there isn't one
        /// </summary>
        /// <returns></returns>
        new Dictionary<string, int> GetDeckEffects();
    }

}
