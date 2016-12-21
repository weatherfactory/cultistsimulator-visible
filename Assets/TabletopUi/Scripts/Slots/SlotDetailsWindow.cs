using System.Collections.Generic;
using Assets.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.TabletopUi.Scripts;

namespace Assets.CS.TabletopUI {
    public class SlotDetailsWindow : MonoBehaviour {

        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] CanvasGroupFader canvasGroupFader;
        [SerializeField] TextMeshProUGUI title;

        [SerializeField] GameObject greedyInfo;
        [SerializeField] GameObject consumesInfo;

        [SerializeField] AspectsDisplay requiresAspectParent;
        [SerializeField] AspectsDisplay forbiddenAspectParent;

        public void Awake() {
            Invoke("Hide", 5);
        }

        public void SetSlot(SlotSpecification slotSpecification) {
            title.text = slotSpecification.Label + slotSpecification.Description;
            
            greedyInfo.gameObject.SetActive(slotSpecification.Greedy);
            consumesInfo.gameObject.SetActive(slotSpecification.Consumes);

            DisplayAspects(requiresAspectParent, slotSpecification.Required);
            DisplayAspects(forbiddenAspectParent, slotSpecification.Forbidden);
        }

        void DisplayAspects(AspectsDisplay display, IAspectsDictionary aspects) {
            display.DisplayAspects(aspects);
        }

        public void Hide() {
            canvasGroupFader.Hide();
        }

    }
}
