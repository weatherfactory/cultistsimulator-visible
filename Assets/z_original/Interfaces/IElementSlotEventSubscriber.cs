using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


    public interface IElementSlotEventSubscriber
    {
        void ElementAddedToSlot(Element element,SlotReceiveElement slot);
        void ElementCannotBeAddedToSlot(Element element, ElementSlotMatch match);
    }

