using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Ghosts
{
    public abstract class AbstractGhost: MonoBehaviour
    {
        
        protected RectTransform rectTransform;
        [SerializeField] protected CanvasGroup canvasGroup;
        public void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public virtual void ShowAt(RectTransform parentTransform, Vector3 anchoredPosition3D)
        {
            canvasGroup.alpha = 1f;
            rectTransform.SetParent(parentTransform);
            rectTransform.anchoredPosition3D = anchoredPosition3D;
        }

        public virtual void HideIn(Token forToken)
        {
            canvasGroup.alpha = 0f;
            rectTransform.SetParent(forToken.TokenRectTransform); //so it doesn't clutter up the hierarchy
        }

        public static AbstractGhost Create(IManifestation manifestation)
        {
            var newGhost=Watchman.Get<PrefabFactory>()
                .CreateGhostPrefab(manifestation.GhostType,manifestation.RectTransform);
            return newGhost;
        }

    }
}