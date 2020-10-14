using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
    public interface IElementManifestation
    {
        void DisplayVisuals(Element element);
        void UpdateText(Element element, int quantity);
        void ResetAnimations();
        bool Retire(CanvasGroup canvasGroup);
        void SetVfx(CardVFX vfxName);
        void ShowGlow(bool glowState, bool instant = false);
        void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval,bool currentlyBeingDragged);
        
    }
}
