#pragma warning disable 0649
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace Assets.CS.TabletopUI {
    public class RecipeSlotIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField] Image image;

        bool isHovering;

        // to check if we're dragging the badge or the stack when starting the drag
        public bool IsHovering() {
            return isHovering;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            isHovering = true;

            // only highlight if we're not dragging anything
            if (!eventData.dragging)
                image.color = UIStyle.hoverWhite;
        }

        public void OnPointerExit(PointerEventData eventData) {
            isHovering = false;
            image.color = UIStyle.slotDefault;
        }

    }
}
