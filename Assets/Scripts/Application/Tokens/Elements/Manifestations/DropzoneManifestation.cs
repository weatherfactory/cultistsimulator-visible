using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Constants;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.Elements.Manifestations
{
    [RequireComponent(typeof(RectTransform))]
    public class DropzoneManifestation: MonoBehaviour,IManifestation
    {
        private GameObject shadow;
        private GraphicFader glowImage;

        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();

        public bool RequestingNoDrag => false;
        public void DoMove(RectTransform tokenRectTransform)
        {
        }


        public void InitialiseVisuals(Element element)
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

        public void InitialiseVisuals(IVerb verb)
        {
        //
        }

        public void UpdateVisuals(Element element, int quantity)
        {
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

        public bool HandlePointerDown(PointerEventData eventData, Token token)
        {
            return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
        //
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
