using UnityEngine;

namespace Assets.CS.TabletopUI
{
    public class DeactivateOnPlay : MonoBehaviour {

        public bool doNotDeactivateIfDebug = false;

        void Start () {
#if DEBUG
            if (doNotDeactivateIfDebug)
                return;
#endif
            gameObject.SetActive(false);
        }
    }
}
