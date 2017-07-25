using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;

namespace Assets.Core.Commands
{
    public class HeartbeatResponse
    {        
        public HashSet<TokenAndSlot> SlotsToFill=new HashSet<TokenAndSlot>();
    }
}
