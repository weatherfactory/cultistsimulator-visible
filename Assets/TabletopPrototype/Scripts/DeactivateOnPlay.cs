using UnityEngine;

namespace Assets.CS.TabletopUI
{
    public class DeactivateOnPlay : MonoBehaviour {
        void Start () {
            gameObject.SetActive(false);
        }
    }
}
