using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts.Interfaces
{
    public interface IElementSlotEventSubscriber
    {
        void ElementAddedToSlot(Element element);
    }
}
