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

        const string aspectHeader = "Aspect: ";

        // These are saved here to make sure we have a ref when we're kicking off the anim
        Element element;

        public void ShowAspectDetails(Element element) {
            // Check if we'd show the same, if so: do nothing
            if (this.element == element)
                return;

            this.element = element;
            Show();
        }

        override protected void UpdateContent() {
            if (element != null)
                SetAspect(element);
        }

        void SetAspect(Element element) {
            ShowImage(ResourcesManager.GetSpriteForAspect(element.Id));
            ShowText(aspectHeader + element.Label, element.Description);
        }
    }
}
