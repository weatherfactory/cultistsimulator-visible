using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
    public interface IAnchorManifestation
    {
        void InitialiseVisuals(IVerb verb);
        void ResetAnimations();
        bool Retire(CanvasGroup canvasGroup);
        void SetVfx(CardVFX vfxName);
        void UpdateTimerVisuals(float duration, float timeRemaining, EndingFlavour signalEndingFlavour);
        void BeginArtAnimation();
        bool CanAnimate();
        void OnBeginDragVisuals();
        void OnEndDragVisuals();
        void Highlight(HighlightType highlightType);
        void Unhighlight(HighlightType highlightType);
        bool NoPush { get; }
        void DoRevealEffect(bool instant);
        void DoShroudEffect(bool instant);
        bool RequestingNoDrag { get; }
        void ShowMiniSlot(bool greedy);
        void HideMiniSlot();
        void DisplayStackInMiniSlot(ElementStackToken stack);
        void DisplayComplete();
        void SetCompletionCount(int count);
        void ReceiveAndRefineTextNotification(INotification notification);
        void Clicked(PointerEventData eventData,VerbAnchor anchor);
        void OverrideIcon(string icon);
        Vector3 GetOngoingSlotPosition();

        /// <summary>
        /// needs to be set to initial token container
        /// </summary>
        /// <param name="transform"></param>
        void SetParticleSimulationSpace(Transform transform);

        void AnimateTo(IAnimatableToken token, float duration, Vector3 startPos, Vector3 endPos,
            Action<VerbAnchor> SituationAnimDone, float startScale = 1f, float endScale = 1f);
    }

}
