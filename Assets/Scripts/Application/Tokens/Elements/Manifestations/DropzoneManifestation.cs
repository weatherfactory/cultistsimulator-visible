using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;

using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Ghosts;
using SecretHistories.Services;
using SecretHistories.UI;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.Manifestations
{
    [RequireComponent(typeof(RectTransform))]
    public class DropzoneManifestation: MonoBehaviour,IManifestation,IPointerClickHandler, IPointerEnterHandler
    {
        private GraphicFader glowImage;

        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();

        public bool RequestingNoDrag => false;
        [SerializeField] private BubbleSphere bubbleSphere;



        public void OnPointerEnter(PointerEventData eventData)
        {
            bubbleSphere.Pop(new Context(Context.ActionSource.Unknown));

            ExecuteEvents.Execute<IPointerEnterHandler>(transform.parent.gameObject, eventData,
                (parentToken, y) => parentToken.OnPointerEnter(eventData));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            bubbleSphere.Pop(new Context(Context.ActionSource.Unknown));
        }


        public void DoMove(RectTransform tokenRectTransform)
        {
        }

    
        public void InitialiseVisuals(IManifestable manifestable)
        {
            var bubbleSphereSpec = new SphereSpec(typeof(BubbleSphere), "classicdropzonebubble");

            bubbleSphere.ApplySpec(bubbleSphereSpec);

            Watchman.Get<HornedAxe>().RegisterSphere(bubbleSphere);
            manifestable.AttachSphere(bubbleSphere);
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
            //
        }


        public void SendNotification(INotification notification)
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
        return new NullGhost();
        }

        public void OverrideIcon(string icon)
        {
            throw new NotImplementedException();
        }

        public void SetParticleSimulationSpace(Transform transform)
        {
        //
        }

        public void ResetIconAnimation()
        {
            
        }

        public void Retire(RetirementVFX vfx, Action callbackOnRetired)
        {
            Destroy(gameObject);
            callbackOnRetired();
        }


        public void ShowGlow(bool glowState, bool instant = false)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval, bool currentlyBeingDragged)
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

        public void BeginIconAnimation()
        {
        }

        public bool CanAnimateIcon()
        {
            return false;
        }

        public void OnBeginDragVisuals()
        {
        }

        public void OnEndDragVisuals()
        {
        }


        public void Highlight(HighlightType highlightType)
        {
        }

        public void Unhighlight(HighlightType highlightType)
        {
            
        }

        public bool NoPush => true;
        public void Reveal(bool instant)
        {
        }

        public void Shroud(bool instant)
        {
        }



    }
}
