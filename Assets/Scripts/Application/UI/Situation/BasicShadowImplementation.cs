#pragma warning disable 0649
using UnityEngine;

namespace SecretHistories.UI
{
    public class BasicShadowImplementation : MonoBehaviour {

        [SerializeField] float shadowOffset = -5;


        // Shitty shadow also has some issues with casting shadow on things that are higher up. card casting shadow on window does not work visually.
        // possibly multiple seperate shadow objects, that are independent from the card display, but that are subscribed to the biz object? And thus updated whenever it moves?
        // one shadow object on the window and it is masked by the window?

        [SerializeField] RectTransform shadow;

        // This only works if the card is a child of a main group directly, not if it's parented to a window.
        
        public void DoMove (RectTransform shadowCaster) {
            shadow.anchoredPosition3D = new Vector3(shadowOffset, shadowOffset, -shadowCaster.anchoredPosition3D.z);
        }
        
    }
}
