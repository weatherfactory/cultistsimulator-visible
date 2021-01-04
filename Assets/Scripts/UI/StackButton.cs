using SecretHistories.UI;
using SecretHistories.Interfaces;
using UnityEngine;

namespace TabletopUi.Scripts.UI
{
    public class StackButton : MonoBehaviour
    {
        public void StackCards()
        {
            var localNexus = Registry.Get<LocalNexus>();
            localNexus.StackCardsEvent.Invoke();
        }
    }
}