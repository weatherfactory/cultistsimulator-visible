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
using Noon;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
    [RequireComponent(typeof(RectTransform))]
    public class PortalManifestation : MonoBehaviour, IManifestation
    {
        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();


        public void Retire(RetirementVFX retirementVfx, Action callback)
        {
            Destroy(gameObject);
            callback();
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

        public bool NoPush => true;
        public void Reveal(bool instant)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void Shroud(bool instant)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void Emphasise()
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void Understate()
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

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

        public void InitialiseVisuals(Element element)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void InitialiseVisuals(IVerb verb)
        {
            this.transform.position = Vector3.zero;
        }

        public void UpdateVisuals(Element element, int quantity)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void UpdateTimerVisuals(float originalDuration, float durationRemaining, float interval, bool resaturate,
            EndingFlavour signalEndingFlavour)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void SendNotification(INotification notification)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
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

        public bool HandlePointerDown(PointerEventData eventData, Token token)
        {
            return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
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
