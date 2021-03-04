using SecretHistories.Commands;
using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.Spheres;
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

        public bool Visible { get; protected set; }

        public virtual void ShowAt(Sphere projectInSphere, Vector3 anchoredPosition3D)
        {
            canvasGroup.alpha = 1f;
            rectTransform.SetParent(projectInSphere.GetRectTransform());
            rectTransform.anchoredPosition3D = anchoredPosition3D;
            Visible = true;
        }

        public virtual void HideIn(Token forToken)
        {
            canvasGroup.alpha = 0f;
            rectTransform.SetParent(forToken.TokenRectTransform); //so it doesn't clutter up the hierarchy
            Visible = false;
        }

        public static AbstractGhost Create(IManifestation manifestation)
        {
            var newGhost=Watchman.Get<PrefabFactory>()
                .CreateGhostPrefab(manifestation.GhostType,manifestation.RectTransform);
            return newGhost;
        }

        public bool TryFulfilPromise(Token token,Context context)
        {
            if (!Visible)
                return false; //if the ghost isn't active, there's no promise to fulfill.

            //otherwise, we did show the ghost, so we'd better be ready to make good on it.
            TokenTravelItinerary travellingToGhost =
                new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D, rectTransform.anchoredPosition3D)
                    .WithDuration(0.25f); //should be NoonConstants.MOMENT...

            travellingToGhost.Depart(token, context);

            HideIn(token); //now clean up the ghost

            //and say that we've fulfilled the promise
            return true;
        }

    }
}