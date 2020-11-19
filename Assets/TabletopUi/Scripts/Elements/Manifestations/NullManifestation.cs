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
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
    public class NullManifestation: IManifestation
    {

        public Transform Transform => null;

        public void Retire(RetirementVFX vfx, Action callbackOnRetired)
        {
            callbackOnRetired();
        }

        public bool CanAnimateIcon()
        {
            return false;
        }

        public void BeginIconAnimation()
        {
            //
        }

        public void ResetIconAnimation()
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

        public bool NoPush
        {
            get { return false; }
        }

        public void Reveal(bool instant)
        {
            //
        }

        public void Shroud(bool instant)
        {
         //
        }

        public void Emphasise()
        {
         //
        }

        public void Understate()
        {
          //
        }

        public void DoRevealEffect(bool instant)
        {
            //
        }

        public void DoShroudEffect(bool instant)
        {
            //
        }

        public bool RequestingNoDrag
        {
            get { return true; }
        }
        public void DoMove(RectTransform tokenRectTransform)
        {
            //
        }

        public void InitialiseVisuals(Element element)
        {
      //
        }

        public void InitialiseVisuals(IVerb verb)
        {
            //
        }

        public void UpdateVisuals(Element element, int quantity)
        {
        //
        }

        public void UpdateTimerVisuals(float originalDuration, float durationRemaining, float interval, bool resaturate,
            EndingFlavour signalEndingFlavour)
        {
          //
        }

        public void SendNotification(INotification notification)
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

        public void DisplayStackInMiniSlot(ElementStack stack)
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

        public bool HandleClick(PointerEventData eventData, Token token)
        {
            return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
           //
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

        public void TravelTo(Token token, float duration, Vector3 startPos, Vector3 endPos,
            Action travelComplete,
            float startScale = 1f, float endScale = 1f)
        {
            //
        }
    }
}
