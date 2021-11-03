using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Ghosts;
using SecretHistories.Spheres;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations
{
    [RequireComponent(typeof(RectTransform))]
    public class NullManifestation: MonoBehaviour, IManifestation
    {
        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();



        public void Retire(RetirementVFX vfx, Action callbackOnRetired)
        {
            Watchman.Forget<NullManifestation>();
            Destroy(gameObject);
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

        public void Unshroud(bool instant)
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


        public bool RequestingNoDrag
        {
            get { return true; }
        }

        public bool RequestingNoSplit => false;

        public void DoMove(RectTransform tokenRectTransform)
        {
            //
        }

        public void Initialise(IManifestable manifestable)
        {
      //
        }

        
        public void UpdateVisuals(IManifestable manifestable)
        {
          //
        }

        public void UpdateVisuals(ITokenPayload payload)
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

        public bool HandlePointerDown(PointerEventData eventData, Token token)
        {
            return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
           //
        }

        public IGhost CreateGhost()
        {
            return NullGhost.Create(this);
        }

        public void OverrideIcon(string icon)
        {
            //
        }

        public Vector3 GetOngoingSlotPosition()
        {
            return Vector3.zero;
        }


    }
}
