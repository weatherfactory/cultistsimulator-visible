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
        [SerializeField] public Image icon;

        [SerializeField] private ElementFrame elementFrame;

        public void InitialiseVisuals(Element element)
        {
            Sprite sprite = ResourcesManager.GetSpriteForElement(element.Icon);
            icon.sprite = sprite;

            if (sprite == null)
                icon.color = Color.clear;
            else
                icon.color = Color.white;

            name = "Stored_" + element.Id;
            
            
        }

        public void UpdateVisuals(Element element, int quantity)
        {
            throw new NotImplementedException();
        }

        public void ResetAnimations()
        {
   //
        }

        public bool Retire(RetirementVFX retirementVfx)
        {
            throw new NotImplementedException();
        }

        public void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval, bool currentlyBeingDragged)
        {
            //
        }

        public void BeginArtAnimation(string icon)
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
            throw new NotImplementedException();
        }

        public void Unhighlight(HighlightType highlightType)
        {
            throw new NotImplementedException();
        }

        public bool NoPush { get; }
        public void DoRevealEffect(bool instant)
        {
            throw new NotImplementedException();
        }

        public void DoShroudEffect(bool instant)
        {
            throw new NotImplementedException();
        }

        public bool RequestingNoDrag => true;
        public void DoMove(RectTransform tokenRectTransform)
        {
           //
        }
    }
}
