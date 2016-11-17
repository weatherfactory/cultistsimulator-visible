using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.Core.Interfaces
{
    public interface IEffectCommand
    {
        string Title { get; set; }
        string Description { get; set; }
        Recipe Recipe { get; set; }
        Dictionary<string, int> GetElementChanges();

    }

}
