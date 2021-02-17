using SecretHistories.UI;
using SecretHistories.Fucine;
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