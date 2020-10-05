using Assets.Core.Entities;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI
{
    public class TabletopBackground : MonoBehaviour, IDropHandler, IPointerClickHandler {

        public event System.Action<PointerEventData> onDropped;
        public event System.Action<PointerEventData> onClicked;

#pragma warning disable 649
        [SerializeField] private Image Cover;
            [SerializeField] Image Surface;
        [SerializeField] Image Edge;
#pragma warning restore 649


        public void OnDrop(PointerEventData eventData) {
            if (!eventData.dragging)
                return;

            onDropped?.Invoke(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClicked?.Invoke(eventData);
        }

        public void ShowTabletopFor(Legacy characterActiveLegacy)
        {

            if (!string.IsNullOrEmpty(characterActiveLegacy.TableCoverImage))
            {
                var coverImage = ResourcesManager.GetSpriteForUI(characterActiveLegacy.TableCoverImage);
                Cover.sprite = coverImage;
            }

            if (!string.IsNullOrEmpty(characterActiveLegacy.TableEdgeImage))
            {
                var surfaceImage = ResourcesManager.GetSpriteForUI(characterActiveLegacy.TableSurfaceImage);
                Surface.sprite = surfaceImage;
            }


            if (!string.IsNullOrEmpty(characterActiveLegacy.TableEdgeImage))
            {
                var edgeImage = ResourcesManager.GetSpriteForUI(characterActiveLegacy.TableEdgeImage);
                Edge.sprite = edgeImage;
            }


            //if (characterActiveLegacy.Id.ToLower().Contains("exile"))
            //{
            //    var exileLeather = ResourcesManager.GetSprite("ui/", "table_leather_exile");
            //    Cover.sprite = exileLeather;
            //}
        }

    }
}
