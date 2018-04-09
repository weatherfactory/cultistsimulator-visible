﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.Core.Interfaces
{
    public interface INotifier
    {
        void PushTextToLog(string text);
        void ShowNotificationWindow(string title, string description, float duration = 10);
        void ShowCardElementDetails(Element element, ElementStackToken token);
        void ShowElementDetails(Element element, bool fromDetailsWindow = false);
        void ShowDeckDetails(string deckId, int deckQuantity);
        void ShowSlotDetails(SlotSpecification slot, bool highlightGreedy, bool highlightConsumes);
        void ShowImageBurn(string spriteName, DraggableToken token, float duration, float scale, TabletopImageBurner.ImageLayoutConfig alignment);
    }
}
