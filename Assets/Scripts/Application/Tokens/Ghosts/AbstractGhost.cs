using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Ghosts
{
    public abstract class AbstractGhost: MonoBehaviour,IGhost
    {
        
        protected RectTransform rectTransform;
        private Sphere _projectedInSphere;
        [SerializeField] protected CanvasGroupFader canvasGroupFader;
        public void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public bool Visible => canvasGroupFader.IsVisible();

        public virtual void ShowAt(Sphere projectInSphere, Vector3 anchoredPosition3D)
        {
            canvasGroupFader.Show();
            rectTransform.SetParent(projectInSphere.GetRectTransform());
            rectTransform.anchoredPosition3D = anchoredPosition3D;
            _projectedInSphere = projectInSphere;
        }

        public virtual void HideIn(Token forToken)
        {
            canvasGroupFader.Hide();
            if(rectTransform!=null)
                rectTransform.SetParent(forToken.TokenRectTransform); //so it doesn't clutter up the hierarchy
            _projectedInSphere = null;

        }

        public bool PromiseBlocksCandidateRect(Sphere sphere, Rect candidateRect)
        {
            if (!Visible)
                return false; //invisible ghosts never block nuthin
            if (sphere != _projectedInSphere)
                return false;
            if (!GetRect().Overlaps(candidateRect))
                return false;

            return true;

        }

        public Rect GetRect()
        {
            var rt = gameObject.GetComponent<RectTransform>();
            if(rectTransform==null)
                return new Rect(0,0,0,0);

            return rt.rect;
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



        public virtual void Retire()
        {
            Destroy(gameObject);
        }

    }
}