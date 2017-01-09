using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.Core.Interfaces
{
    public interface IEffectCommand
    {
        string Title { get; }
        string Description { get; }
        Recipe Recipe { get; }
        Dictionary<string, int> GetElementChanges();
        bool AsNewSituation { get; }

    }

}
