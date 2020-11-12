using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;

namespace Assets.TabletopUi
{
    public class AnchorAndSlot
    {
        public AbstractToken Token { get; set; } 
        public Sphere Threshold { get; set; }
    }
}
