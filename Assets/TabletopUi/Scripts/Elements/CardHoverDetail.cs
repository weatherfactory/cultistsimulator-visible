using Assets.CS.TabletopUI;
using UnityEngine;

namespace TabletopUi.Scripts.Elements
{
    public class CardHoverDetail : ElementStackSimple, ICanvasRaycastFilter
    {
        [SerializeField] 
        private RectTransform statusBar;
        
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

        private void UpdatePosition()
        {
            var canvas = GetComponentInParent<Canvas>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                ClampMousePosition(canvas, Input.mousePosition),
                canvas.worldCamera,
                out var position);
            
            transform.position = canvas.transform.TransformPoint(position);
        }

        private Vector2 ClampMousePosition(Canvas canvas, Vector2 position)
        {
            // Note: this code isn't entirely correct, but it does the job well
            // enough. - MG
            var scale = canvas.scaleFactor;
            var rect = ((RectTransform) transform).rect;
            var width = rect.width * scale;
            var height = rect.height * scale;
            return new Vector2(
                Mathf.Clamp(
                    position.x, 
                    width,
                    canvas.worldCamera.pixelWidth - width),
                Mathf.Clamp(
                    position.y, 
                    height + statusBar.rect.height * scale,
                    canvas.worldCamera.pixelHeight - height));
        }
    }
}