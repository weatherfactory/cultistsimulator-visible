using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TabletopUi.Scripts.UI
{
    public class BackgroundAdjusterForText : MonoBehaviour
    {
        [SerializeField] private RectTransform backgroundMidContainer;

        [SerializeField] private Image backgroundMidLine;

        [SerializeField] private TextMeshProUGUI text;

        [SerializeField] private int minimumLines;

        public void Adjust()
        {
            text.ForceMeshUpdate();
            var midLineRect = backgroundMidLine.GetComponent<RectTransform>().rect;
            var numChildren = (int) (text.renderedHeight/midLineRect.height) + 1;
            if (numChildren < minimumLines + 1)
            {
                numChildren = minimumLines + 1;
            }
            
            foreach (Transform mid in backgroundMidContainer.Cast<Transform>().ToArray())
                DestroyImmediate(mid.gameObject);

            for (int i = 0; i < numChildren; i++)
            {
                var newLine = Instantiate(backgroundMidLine, backgroundMidContainer, false);
                newLine.gameObject.SetActive(true);
            }

            LayoutRebuilder.MarkLayoutForRebuild(backgroundMidContainer);
        }
    }
}
