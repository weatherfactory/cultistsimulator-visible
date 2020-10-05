using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TabletopUi.Scripts.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextWithHyperlinks : MonoBehaviour, IPointerClickHandler
    {
        public bool doesColorChangeOnHover = true;
        public Color hoverColor = new Color(60f / 255f, 120f / 255f, 1f);

        private TextMeshProUGUI _text;
        private Canvas _canvas;
        private Camera _camera;

        private int _currentLink = -1;
        private List<Color32[]> _originalVertexColors = new List<Color32[]>();

        protected void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _canvas = GetComponentInParent<Canvas>();

            _camera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        }

        private void LateUpdate()
        {
            //var isHoveringOver =
            //    TMP_TextUtilities.IsIntersectingRectTransform(_text.rectTransform, Input.mousePosition, _camera);
            //int linkIndex;
            //try
            //{
            //    linkIndex = isHoveringOver
            //        ? TMP_TextUtilities.FindIntersectingLink(_text, Input.mousePosition, _camera)
            //        : -1;
            //}
            //catch (IndexOutOfRangeException)
            //{
            //    linkIndex = -1;
            //}

            //if (_currentLink != -1 && linkIndex != _currentLink)
            //{
            //    SetLinkToColor(_currentLink, (linkIdx, vertIdx) => _originalVertexColors[linkIdx][vertIdx]);
            //    _originalVertexColors.Clear();
            //    _currentLink = -1;
            //}

            //if (linkIndex != -1 && linkIndex != _currentLink)
            //{
            //    _currentLink = linkIndex;
            //    if (doesColorChangeOnHover)
            //        _originalVertexColors = SetLinkToColor(linkIndex, (linkIdx, vertIdx) => hoverColor);
            //}
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, Input.mousePosition, _camera);
            if (linkIndex == -1) 
                return;
            TMP_LinkInfo linkInfo = _text.textInfo.linkInfo[linkIndex];

            Application.OpenURL(linkInfo.GetLinkID());
        }

        private List<Color32[]> SetLinkToColor(int linkIndex, Func<int, int, Color32> colorForLinkAndVert)
        {
            TMP_LinkInfo linkInfo = _text.textInfo.linkInfo[linkIndex];

            var oldVertColors = new List<Color32[]>();

            for (int i = 0; i < linkInfo.linkTextLength; i++)
            {
                int characterIndex = linkInfo.linkTextfirstCharacterIndex + i;
                var charInfo = _text.textInfo.characterInfo[characterIndex];
                int meshIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                Color32[] vertexColors = _text.textInfo.meshInfo[meshIndex].colors32;
                oldVertColors.Add(vertexColors.ToArray());

                if (!charInfo.isVisible) 
                    continue;
                vertexColors[vertexIndex + 0] = colorForLinkAndVert(i, vertexIndex + 0);
                vertexColors[vertexIndex + 1] = colorForLinkAndVert(i, vertexIndex + 1);
                vertexColors[vertexIndex + 2] = colorForLinkAndVert(i, vertexIndex + 2);
                vertexColors[vertexIndex + 3] = colorForLinkAndVert(i, vertexIndex + 3);
            }

            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

            return oldVertColors;
        }
    }
}