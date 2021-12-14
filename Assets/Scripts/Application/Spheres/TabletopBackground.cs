using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Entities;
using SecretHistories.Constants;
using SecretHistories.Spheres;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.UI
{
    public class TabletopBackground : AbstractBackground{

      

#pragma warning disable 649
        [SerializeField] private Image Cover;
            [SerializeField] Image Surface;
        [SerializeField] Image Edge;
#pragma warning restore 649







        public override void ShowBackgroundFor(Legacy characterActiveLegacy)
        {

            if (!string.IsNullOrEmpty(characterActiveLegacy.TableCoverImage))
            {
                var coverImage = ResourcesManager.GetSpriteForUI(characterActiveLegacy.TableCoverImage);
                Cover.sprite = coverImage;
            }

            if (!string.IsNullOrEmpty(characterActiveLegacy.TableSurfaceImage))
            {
                var surfaceImage = ResourcesManager.GetSpriteForUI(characterActiveLegacy.TableSurfaceImage);
                Surface.sprite = surfaceImage;
            }


            if (!string.IsNullOrEmpty(characterActiveLegacy.TableEdgeImage))
            {
                var edgeImage = ResourcesManager.GetSpriteForUI(characterActiveLegacy.TableEdgeImage);
                Edge.sprite = edgeImage;
            }

        }

    }
}
