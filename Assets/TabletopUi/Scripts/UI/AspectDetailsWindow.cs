#pragma warning disable 0649
using System.Collections.Generic;
using Assets.Core;
using Assets.TabletopUi.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace Assets.CS.TabletopUI {
    public class AspectDetailsWindow : BaseDetailsWindow {
        
        [SerializeField] Vector2 posNoTokenDetails = new Vector2(0f, 0f);
        [SerializeField] Vector2 posWithTokenDetails = new Vector2(0f, -220f);

        const string aspectHeader = "Aspect: ";

        // These are saved here to make sure we have a ref when we're kicking off the anim
        Element element;
        bool isPositionedAbove;

        public void ShowAspectDetails(Element element, bool postionAbove) {
            // Check if we'd show the same, if so: do nothing
            if (this.element == element && gameObject.activeSelf && postionAbove == isPositionedAbove)
                return;

            Debug.Log("Position" + (transform as RectTransform).anchoredPosition);

            this.isPositionedAbove = postionAbove;
            this.element = element;
            Show();
        }

        protected override void ClearContent() {
            this.element = null;
        }

        override protected void UpdateContent() {
            if (element != null)
                SetAspect(element);

            (transform as RectTransform).anchoredPosition = isPositionedAbove ? posNoTokenDetails : posWithTokenDetails;
        }

        void SetAspect(Element element) {
            ShowImage(ResourcesManager.GetSpriteForAspect(element.Id));
            ShowText(aspectHeader + element.Label, element.Description);
        }
    }
}
