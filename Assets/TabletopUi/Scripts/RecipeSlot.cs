using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.CS.TabletopUI
{
    public class RecipeSlot : MonoBehaviour, IDropHandler {

        public event System.Action<RecipeSlot> onCardDropped;

        // TODO: Needs hover feedback!

        public void OnDrop(PointerEventData eventData) {
            if (onCardDropped != null)
                onCardDropped(this);
        }
    }
}
