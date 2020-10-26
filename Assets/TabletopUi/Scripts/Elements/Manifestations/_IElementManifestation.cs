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



        public interface IElementManifestation
    {
        void InitialiseVisuals(Element element);
        void UpdateText(Element element, int quantity);
        void ResetAnimations();
        bool Retire(CanvasGroup canvasGroup);
        void SetVfx(CardVFX vfxName);
        void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval,bool currentlyBeingDragged);
        void BeginArtAnimation(string icon);
        bool CanAnimate();
        void OnBeginDragVisuals();
        void OnEndDragVisuals();
        void Highlight(HighlightType highlightType);
        void Unhighlight(HighlightType highlightType);
        bool NoPush { get; }
        void DoRevealEffect(bool instant);
        void DoShroudEffect(bool instant);
        bool RequestingNoDrag { get; }
        void AnimateTo(float duration, Vector3 startPos, Vector3 endPos, Action<AbstractToken> animDone, float startScale, float endScale);
    }
}
