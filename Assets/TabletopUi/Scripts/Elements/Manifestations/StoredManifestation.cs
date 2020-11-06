using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
    public class StoredManifestation: MonoBehaviour, IElementManifestation
    {
        [SerializeField] private CanvasGroup canvasGroup;
        
        [SerializeField] private ElementFrame elementFrame;

        public void InitialiseVisuals(Element element)
        {
            name = "StoredManifestation_" + element.Id;
            elementFrame.PopulateDisplay(element, 1, false);
        }

        public void UpdateVisuals(Element element, int quantity)
        {
            elementFrame.PopulateDisplay(element, quantity, false);

        }


        public void ResetAnimations()
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

        public void Emphasise()
        {
            canvasGroup.alpha = 0.3f;
        }

        public void Understate()
        {
            
        }

        public void BeginArtAnimation()
        {
        //
        }

    public bool CanAnimate()
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
