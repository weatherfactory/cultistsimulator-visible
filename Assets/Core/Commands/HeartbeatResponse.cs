using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;

namespace Assets.Core.Commands
{
    public class HeartbeatResponse
    {
        public IList<IRecipeSlot> SlotsToFill=new List<IRecipeSlot>();
    }
}
