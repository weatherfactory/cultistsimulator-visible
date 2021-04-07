using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace SecretHistories.UI
{

    public class HintPanel: MonoBehaviour
    {
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private CanvasGroupFader canvasGroupFader;

        [SerializeField]
        private RectTransform rectTransform;


        public void Awake()
        {
            var w=new Watchman();
            w.Register(this);
        }
        public void ShowHint(Vector3 atPosition, string hint)
        {
            canvasGroupFader.Show();
            rectTransform.position = atPosition;
            hintText.text = hint;
        }

        public void Hide()
        {
            canvasGroupFader.Hide();
            hintText.text = string.Empty;
        }
    }
}
