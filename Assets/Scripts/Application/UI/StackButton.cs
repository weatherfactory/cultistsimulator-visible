using SecretHistories.UI;
using SecretHistories.Interfaces;
using UnityEngine;

namespace SecretHistories.Enums.UI
{
    public class StackButton : MonoBehaviour
    {
        public void StackCards()
        {
            var localNexus = Watchman.Get<LocalNexus>();
            localNexus.StackCardsEvent.Invoke();
        }
    }
}