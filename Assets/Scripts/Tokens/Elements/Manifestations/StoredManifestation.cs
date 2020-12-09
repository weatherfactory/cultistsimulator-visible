using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{

#pragma warning disable 649
    [RequireComponent(typeof(RectTransform))]
    public class StoredManifestation: MonoBehaviour, IManifestation
    {
        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();


        [SerializeField] private CanvasGroup canvasGroup;

        
        [SerializeField] private ElementFrame elementFrame;

#pragma warning restore 649
        public void InitialiseVisuals(Element element)
        {
            name = "StoredManifestation_" + element.Id;
            elementFrame.PopulateDisplay(element, 1, false);
        }

        public void InitialiseVisuals(IVerb verb)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support InitialiseVisuals");
        }

        public void UpdateVisuals(Element element, int quantity)
        {
            elementFrame.PopulateDisplay(element, quantity, false);

        }

        public void UpdateTimerVisuals(float originalDuration, float durationRemaining, float interval, bool resaturate,
            EndingFlavour signalEndingFlavour)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support UpdateTimerVisuals");
        }

        public void SendNotification(INotification notification)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support SendNotification");
        }

        public bool HandlePointerDown(PointerEventData eventData, Token token)
        {
            return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support DisplaySpheres");
        }

        public void OverrideIcon(string icon)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support OverrideIcon");
        }

        public void SetParticleSimulationSpace(Transform transform)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support SetParticleSimulationSpace");
        }


        public void ResetIconAnimation()
        {
   //
        }


        public void Retire(RetirementVFX vfx, Action callbackOnRetired)
        {
            Destroy(gameObject);
            callbackOnRetired();
        }

        public void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval, bool currentlyBeingDragged)
        {
            //
        }

        public void Reveal(bool instant)
        {
            throw new NotImplementedException();
        }

        public void Shroud(bool instant)
        {
            throw new NotImplementedException();
        }

        public void Emphasise()
        {
            canvasGroup.alpha = 0.3f;
        }

        public void Understate()
        {
            
        }

        public void BeginIconAnimation()
        {
        //
        }

    public bool CanAnimateIcon()
        {
           return false;
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

        public bool NoPush { get; }
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
    }
}
