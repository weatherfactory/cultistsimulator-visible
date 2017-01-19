using System.Collections.Generic;
using Assets.Core;
using Assets.TabletopUi.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI
{
    public class ElementDetailsWindow : MonoBehaviour, IPointerClickHandler{

        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI description;
        [SerializeField] TextMeshProUGUI slots;
        [SerializeField] AspectsDisplay aspectsDisplay;
        [SerializeField] private CanvasGroupFader canvasGroupFader;

        public void Awake()
        {
            Invoke("Hide", 10);
        }

        public void SetElementCard(Element element) {

         var elementSprite= ResourcesManager.GetSpriteForElement(element.Id);
            artwork.sprite = elementSprite;
         title.text = element.Label;
            description.text = element.Description; 
          aspectsDisplay.DisplayAspects(element.Aspects);

        }

        string GetSlotsText(List<SlotSpecification> slots) { // THis could be in a TOString methodto be more accessible where it's needed?
            if (slots == null || slots.Count == 0)
                return "Slots: None";

            var stringBuilder = new System.Text.StringBuilder("Slots: "+slots.Count +"\n");

            for (int i = 0; i < slots.Count; i++) {
                stringBuilder.Append(slots[i].Id);

                if (slots[i].Required.Count > 0 || slots[i].Forbidden.Count > 0)
                    stringBuilder.Append(" (");

                if (slots[i].Required.Count > 0) {
                    stringBuilder.Append("Required: ");
                    stringBuilder.Append(GetAspectsText(slots[i].Required));
                }

                if (slots[i].Required.Count > 0 && slots[i].Forbidden.Count > 0)
                    stringBuilder.Append(" | ");
			
                if (slots[i].Forbidden.Count > 0) {
                    stringBuilder.Append("Forbidden: ");
                    stringBuilder.Append(GetAspectsText(slots[i].Forbidden));
                }

                if (slots[i].Required.Count > 0 || slots[i].Forbidden.Count > 0)
                    stringBuilder.Append(")");

                if (i + 1 < slots.Count)
                    stringBuilder.Append("\n");
            }

            return stringBuilder.ToString();
        }

        string GetAspectsText(IAspectsDictionary aspects) {
            if (aspects == null || aspects.Count == 0)
                return "None.";

            var stringBuilder = new System.Text.StringBuilder();
            int i = 0;

            foreach (var keyValuePair in aspects) {
                stringBuilder.Append(keyValuePair.Key);
                stringBuilder.Append(" ");
                stringBuilder.Append(keyValuePair.Value);
                i++;

                if (i < aspects.Count)
                    stringBuilder.Append(", ");
            }

            return stringBuilder.ToString();
        }

        public void Hide() {
          canvasGroupFader.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Hide();
        }
    }
}
