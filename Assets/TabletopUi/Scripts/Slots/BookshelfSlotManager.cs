using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;

namespace Assets.TabletopUi.Scripts.Slots
{
   public class BookshelfSlotManager: AbstractSlotsManager
    {
        public override void RespondToStackRemoved(IElementStack stack, Context context)
        {
            throw new NotImplementedException();
        }

        public override void RespondToStackAdded(RecipeSlot slot, IElementStack stack, Context context)
        {
            throw new NotImplementedException();
        }
    }
}
