using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.UI
{
    public class DraggableHolder: IDraggableHolder
    {
        public RectTransform RectTransform { get; private set; }

        public DraggableHolder(RectTransform rectTransform)
        {
            RectTransform = rectTransform;
        }
    }
}
