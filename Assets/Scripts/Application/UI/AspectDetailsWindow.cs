#pragma warning disable 0649
using System.Collections.Generic;
using SecretHistories.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Assets.Scripts.Application.Spheres;
using SecretHistories.Entities;
using SecretHistories.Enums.UI;

namespace SecretHistories.UI {
    public class AspectDetailsWindow : AbstractDetailsWindow {

		[SerializeField] RectTransform tokenDetailsHeight;
        [SerializeField] Vector2 posNoDetailsWindow = new Vector2(0f, 0f);
        [SerializeField] private BackgroundAdjusterForText adjuster;

        // These are saved here to make sure we have a ref when we're kicking off the anim
        Element element;

        public void ShowAspectDetails(Element element, bool fromDetailsWindow) {
            // Check if we'd show the same, if so: do nothing
            if (this.element == element && gameObject.activeSelf)
                return;


            this.element = element;
            Show();
        }

        protected override void ClearContent() {
            this.element = null;
        }

        protected override void UpdateContentAfterNavigation(NavigationArgs args)
        {
            UpdateContent();
        }

        protected override void UpdateContent() {
            if (element != null)
                SetAspect(element);

			//if (!fromDetailsWindow)
			//	(transform as RectTransform).anchoredPosition = posNoDetailsWindow;
			//else
				(transform as RectTransform).anchoredPosition = new Vector2( 0f, -tokenDetailsHeight.sizeDelta.y - 10f);

		//	Debug.Log("tokenDetails size : "+ tokenDetailsHeight.sizeDelta.y);
        }

        void SetAspect(Element element)
		{
            if(element.IsAspect)
                ShowImage(ResourcesManager.GetSpriteForAspect(element.Icon));
            else
                ShowImage(ResourcesManager.GetSpriteForElement(element.Icon));

            ShowText(Watchman.Get<ILocStringProvider>().Get("UI_ASPECT") + element.Label, element.Description);
            adjuster.Adjust();
        }
    }
}
