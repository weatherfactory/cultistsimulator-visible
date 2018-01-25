#pragma warning disable 0649
using UnityEngine;

namespace Assets.CS.TabletopUI
{
    public class BasicShadowImplementation : MonoBehaviour {

        // This is not a good class.
        // Ideally the ElementCard would set it's shadow whenever it sets it's position, since then it knows it's z position based on it's current state
        // resting on table, set into window, dragging, slotted into window

        // Shitty shadow also has some issues with casting shadow on things that are higher up. card casting shadow on window does not work visually.
        // possibly multiple seperate shadow objects, that are independent from the card display, but that are subscribed to the biz object? And thus updated whenever it moves?
        // one shadow object on the window and it is masked by the window?

        [SerializeField] RectTransform shadow;
        [SerializeField] RectTransform card;

        // This only works if the card is a child of a main group directly, not if it's parented to a window.
        
        void Update () {
            float zOffsetCard = card.anchoredPosition3D.z;

            shadow.anchoredPosition3D = new Vector3(-5f, -5f, -zOffsetCard);
            shadow.localScale = zOffsetCard == 0 ? Vector3.one : Vector3.one * 1.1f;
        }
        
    }
}
