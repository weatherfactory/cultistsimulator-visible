using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
    /// <summary>
    /// used for an element that has been retired.
    /// </summary>
   public class NullElementManifestation: IElementManifestation
    {
        public void InitialiseVisuals(Element element)
        {
            
        }

        public void UpdateVisuals(Element element, int quantity)
        {
            
        }

        public void ResetAnimations()
        {
            
        }

        public bool Retire(RetirementVFX retirementVfx)
        {
            return true;
        }

        public void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval, bool currentlyBeingDragged)
        {
            
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
        

        public bool RequestingNoDrag => true;
        public void DoRevealEffect(bool instant)
        {
            }

        public void DoShroudEffect(bool instant)
        {
            }

        
        public void DoMove(RectTransform tokenRectTransform)
        {
            
        }
    }
}
