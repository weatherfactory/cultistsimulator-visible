using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Manifestations;
using SecretHistories.Services;
using SecretHistories.UI;
using UnityEditor.Experimental.GraphView;
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

        public void ShowAt(RectTransform parentTransform, Vector3 anchoredPosition3D)
        {
            canvasGroup.alpha = 1f;
            rectTransform.SetParent(parentTransform);
            rectTransform.anchoredPosition3D = anchoredPosition3D;
        }

        public void HideIn(Token forToken)
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

    public class CardGhost: AbstractGhost
    {
 


   
    }
}
