using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
    public enum HighlightType
    {
        CanFitSlot,
        CanMerge,
        All,
        Hover,
        AttentionPls,
        CanInteractWithOtherToken
    }



        public interface IElementManifestation:IManifestation
    {
        void InitialiseVisuals(Element element);
        void UpdateVisuals(Element element, int quantity);
        void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval,bool currentlyBeingDragged);

    }
}
