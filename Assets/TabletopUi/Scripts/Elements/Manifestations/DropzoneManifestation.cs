using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
   public class DropzoneManifestation: MonoBehaviour,IElementManifestation
    {
        private GameObject shadow;
        private GraphicFader glowImage;

        public void DisplayVisuals(Element element)
        {
            // Customize appearance of card to make it distinctive
            // First hide normal card elements
            Transform oldcard = transform.Find("Card");
            Transform oldglow = transform.Find("Glow");
            Transform oldshadow = transform.Find("Shadow");
            if (oldcard)
            {
                oldcard.gameObject.SetActive(false);
            }
            if (oldglow)
            {
                oldglow.gameObject.SetActive(false);
            }
            if (oldshadow)
            {
                oldshadow.gameObject.SetActive(false);
            }

            // Now create an instance of the dropzone prefab parented to this card
            // This way any unused references are still pointing at the original card data, so no risk of null refs.
            // It's a bit hacky, but it's now a live project so refactoring the entire codebase to make it safe is high-risk.
            TabletopManager tabletop = Registry.Get<TabletopManager>() as TabletopManager;

            GameObject zoneobj = GameObject.Instantiate(tabletop._dropZoneTemplate, transform);
            Transform newcard = zoneobj.transform.Find("Card");
            Transform newglow = zoneobj.transform.Find("Glow");
            Transform newshadow = zoneobj.transform.Find("Shadow");

            glowImage = newglow.gameObject.GetComponent<GraphicFader>() as GraphicFader;
            newglow.gameObject.SetActive(false);
            shadow = newshadow.gameObject;

            // Modify original card settings
            LayoutElement layoutElement = GetComponent<LayoutElement>() as LayoutElement;
            if (layoutElement)
            {
                layoutElement.preferredWidth = 0f;    // Do not want this zone to interact with cards at all
                layoutElement.preferredHeight = 0f;
            }
        }

        public void UpdateText(Element element, int quantity)
        {
            throw new NotImplementedException();
        }

        public void ResetAnimations()
        {
            throw new NotImplementedException();
        }

        public bool Retire(CanvasGroup canvasGroup)
        {
            throw new NotImplementedException();
        }

        public void SetVfx(CardVFX vfxName)
        {
            throw new NotImplementedException();
        }

        public void ShowGlow(bool glowState, bool instant = false)
        {
            throw new NotImplementedException();
        }

        public void UpdateDecayVisuals(float lifetimeRemaining, Element element, float interval, bool currentlyBeingDragged)
        {
            throw new NotImplementedException();
        }

        public void BeginArtAnimation(string icon)
        {
            throw new NotImplementedException();
        }

        public bool CanAnimate()
        {
            throw new NotImplementedException();
        }

        public void OnBeginDragVisuals()
        {
            throw new NotImplementedException();
        }

        public void OnEndDragVisuals()
        {
            throw new NotImplementedException();
        }

        public void Highlight(HighlightType highlightType)
        {
            throw new NotImplementedException();
        }

        public bool NoPush => true;
    }
}
