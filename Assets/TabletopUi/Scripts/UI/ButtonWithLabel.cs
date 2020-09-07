using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.UI
{
    public class ButtonWithLabel: MonoBehaviour
    {
        [SerializeField] public Image Image;
        [SerializeField] public TMP_Text Label;
        private UnityEvent _eventToTrigger;

        public void Initialise(UnityEvent eventToTrigger)
        {
            _eventToTrigger = eventToTrigger;
        }

        public void Initialise(Sprite sprite, string labelText, UnityEvent eventToTrigger)
        {
            Image.sprite = sprite;
            Label.text = labelText;
            Initialise(eventToTrigger);
        }

        public void OnClick()
        {
            _eventToTrigger.Invoke();
        }

    }
}
