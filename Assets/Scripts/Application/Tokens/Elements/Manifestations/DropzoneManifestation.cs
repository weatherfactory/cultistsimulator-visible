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
        
        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();

        public bool RequestingNoDrag => false;
        public bool RequestingNoSplit => true;

        [SerializeField] private List<MinimalDominion> Dominions;


        public void OnPointerEnter(PointerEventData eventData)
        {

            popDropzoneBubbles();

            ExecuteEvents.Execute<IPointerEnterHandler>(transform.parent.gameObject, eventData,
                (parentToken, y) => parentToken.OnPointerEnter(eventData));
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            popDropzoneBubbles();

        }

        private void popDropzoneBubbles()
        {
            foreach (var d in Dominions)
            {
                foreach (var s in d.Spheres)
                {
                    var b = s as BubbleSphere;
                    if (b != null)
                        b.Pop(new Context(Context.ActionSource.Unknown));
                }
            }
        }


        public void DoMove(RectTransform tokenRectTransform)
        {
        }

    
        public void Initialise(IManifestable manifestable)
        {
            foreach (var d in Dominions)
                d.RegisterFor(manifestable);
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

        public bool NoPush => false;
        public void Unshroud(bool instant)
        {
        }

        public void Shroud(bool instant)
        {
        }



    }
}
