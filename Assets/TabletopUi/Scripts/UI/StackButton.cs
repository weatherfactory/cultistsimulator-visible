using Assets.CS.TabletopUI;
using TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace TabletopUi.Scripts.UI
{
    public class StackButton : MonoBehaviour
    {
        public void StackCards()
        {
            var tabletop = Registry.Retrieve<ITabletopManager>();
            tabletop.GroupAllStacks();
        }
    }
}