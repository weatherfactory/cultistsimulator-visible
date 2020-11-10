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
   public class PortalManifestation: MonoBehaviour,IAnchorManifestation
    {
        public void Retire(RetirementVFX retirementVfx, Action callback)
        {
            Destroy(gameObject);
            callback();
        }

        public bool CanAnimate()
        {
            return false;
        }

        public void BeginArtAnimation()
        {
         //
        }

        public void ResetAnimations()
        {
      //
        }

        public void OnBeginDragVisuals()
        {
         //
        }

        public void OnEndDragVisuals()
        {
//
        }

        public void Highlight(HighlightType highlightType)
        {
      //
        }

        public void Unhighlight(HighlightType highlightType)
        {
            //
        }

        public bool NoPush => true;

        public void DoRevealEffect(bool instant)
        {
        //
        }

        public void DoShroudEffect(bool instant)
        {
        //
        }

        public bool RequestingNoDrag => true;

        public void DoMove(RectTransform tokenRectTransform)
        {
            //
        }

        public void InitialiseVisuals(IVerb verb)
        {
            //
        }

        public void UpdateTimerVisuals(float duration, float timeRemaining, EndingFlavour signalEndingFlavour)
        {
            //
        }

        public void ShowMiniSlot(bool greedy)
        {
            //
        }

        public void HideMiniSlot()
        {
            //
        }

        public void DisplayStackInMiniSlot(ElementStackToken stack)
        {
            //
        }

        public void DisplayComplete()
        {
            //
        }

        public void SetCompletionCount(int count)
        {
            //
        }

        public void ReceiveAndRefineTextNotification(INotification notification)
        {
            //
        }

        public bool HandleClick(PointerEventData eventData, VerbAnchor anchor)
        {
            return false;
        }

        public void OverrideIcon(string icon)
        {
          //
        }

        public Vector3 GetOngoingSlotPosition()
        {
          return Vector3.zero;
        }

        public void SetParticleSimulationSpace(Transform transform)
        {
          //
        }

        public void AnimateTo(IArtAnimatableToken token, float duration, Vector3 startPos, Vector3 endPos, Action<VerbAnchor> SituationAnimDone,
            float startScale = 1, float endScale = 1)
        {
            //
        }
    }
}
