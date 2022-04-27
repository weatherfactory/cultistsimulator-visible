using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Assets.Scripts.Application.UI.Screens
{
    public class MenuModalDarkening: MonoBehaviour,IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            Watchman.Get<LocalNexus>().HideMenusEvent.Invoke();
        }
    }
}
