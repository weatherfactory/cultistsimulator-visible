using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.UI
{
    public abstract class AbstractBackground: MonoBehaviour,IPointerClickHandler
    {

        public void Awake()
        {
            var w = new Watchman();
            w.Register(this);
        }


        public event System.Action<PointerEventData> onClicked;
        public void OnPointerClick(PointerEventData eventData)
        {
            onClicked?.Invoke(eventData);
        }

        public abstract void ShowBackgroundFor(Legacy characterActiveLegacy);
    }
}
