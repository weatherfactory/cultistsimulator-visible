using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
   public class MinimalManifestation:MonoBehaviour,IElementManifestation
    {
        public bool RequestingNoDrag => false;
        public void DoMove(RectTransform tokenRectTransform)
        {

        }

        public void AnimateTo(float duration, Vector3 startPos, Vector3 endPos, Action<AbstractToken> animDone, float startScale, float endScale)
        {
            //do nothing
        }

        public void InitialiseVisuals(Element element)
        {
            //do nothing
        }

        public void UpdateVisuals(Element element, int quantity)
        {
            //do nothing
        }

        public void ResetAnimations()
        {
            //do nothing
        }

        public void Retire(RetirementVFX vfx, Action callbackOnRetired)
        {
            callbackOnRetired();
        }



        public void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval, bool currentlyBeingDragged)
        {
            }

        public void Emphasise()
        {
            //
        }

        public void Understate()
        {
            //
        }

        public void BeginArtAnimation()
        {
            
        }

        public bool CanAnimate()
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
        public void DoRevealEffect(bool instant)
        {
        }

        public void DoShroudEffect(bool instant)
        {
        }
    }
}
