using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Ghosts
{
    public interface IGhost
    {
        void ShowPosition(Vector3 anchoredPosition3D);
    }

    public class CardGhost: MonoBehaviour,IGhost
    {
        private RectTransform rectTransform;

        public void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        public void ShowPosition(Vector3 anchoredPosition3D)
        {
            rectTransform.anchoredPosition3D = anchoredPosition3D;
        }
    }
}
