using Assets.CS.TabletopUI;
using UnityEngine;

namespace TabletopUi.Scripts.UI
{
    public class StackButton : MonoBehaviour
    {
        public void StackCards()
        {
            var tabletop = Registry.Get<TabletopManager>();
            tabletop.GroupAllStacks();
        }
    }
}