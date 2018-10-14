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
        
		[SerializeField] RectTransform tokenDetailsHeight;
        [SerializeField] Vector2 posNoTokenDetails = new Vector2(0f, 0f);
        [SerializeField] Vector2 posWithTokenDetails = new Vector2(0f, -220f);

        const string aspectHeader = "Aspect: ";

        // These are saved here to make sure we have a ref when we're kicking off the anim
        Element element;
		bool _noTokenDetails;

        public void ShowAspectDetails(Element element, bool noTokenDetails) {
            // Check if we'd show the same, if so: do nothing
            if (this.element == element && gameObject.activeSelf && _noTokenDetails == noTokenDetails)
                return;

            Debug.Log("Position" + (transform as RectTransform).anchoredPosition);

            this._noTokenDetails = noTokenDetails;
            this.element = element;
            Show();
        }

        protected override void ClearContent() {
            this.element = null;
        }

        protected override void UpdateContent() {
            if (element != null)
                SetAspect(element);

			if (_noTokenDetails)
				(transform as RectTransform).anchoredPosition = posNoTokenDetails;
			else 
				(transform as RectTransform).anchoredPosition = new Vector2( 0f, -tokenDetailsHeight.sizeDelta.y - 10f);

			Debug.Log("tokenDetails size : "+ tokenDetailsHeight.sizeDelta.y);
        }

        void SetAspect(Element element) {
            ShowImage(ResourcesManager.GetSpriteForAspect(element.Icon));
            ShowText(aspectHeader + element.Label, element.Description);
        }
    }
}
