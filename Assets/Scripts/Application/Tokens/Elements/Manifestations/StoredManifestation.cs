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
using UnityEngine.UI;

namespace SecretHistories.Manifestations

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



        public void Initialise(IManifestable manifestable)
        {
            name = "StoredManifestation_" + manifestable.Id;
            elementFrame.PopulateDisplay(manifestable.Icon, 1, false);
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
            var element = Watchman.Get<Compendium>().GetEntityById<Element>(manifestable.EntityId);
            elementFrame.PopulateDisplay(element.Icon, manifestable.Quantity, false);
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

        public IGhost CreateGhost()
        {
            return new NullGhost();
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
