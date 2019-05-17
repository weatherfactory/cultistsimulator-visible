using Assets.CS.TabletopUI;
using UnityEngine;

namespace TabletopUi.Scripts.Elements
{
    public class CardHoverDetail : ElementStackSimple, ICanvasRaycastFilter
    {
        public void Show()
        {
            UpdatePosition();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return false;
        }

        public void UpdatePosition()
        {
            var canvas = GetComponentInParent<Canvas>();
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out position);
            transform.position = canvas.transform.TransformPoint(position);
        }
    }
}