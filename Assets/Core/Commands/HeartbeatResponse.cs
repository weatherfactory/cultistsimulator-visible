using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Interfaces;

namespace Assets.Core.Commands
{
    public class HeartbeatResponse
    {        
        public HashSet<AnchorAndSlot> SlotsToFill=new HashSet<AnchorAndSlot>();
        public Token Token { get; set; }
    }
}
