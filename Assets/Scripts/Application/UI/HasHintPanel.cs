using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Assets.Scripts.Application.UI
{
   public class HasHintPanel: MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
   {
       private const float DefaultVerticalOffset = -40f;
       public string Hint;
        public void OnPointerEnter(PointerEventData eventData)
        {
            float verticalOffset;
            RectTransform rt = gameObject.GetComponent<RectTransform>();
            if (rt.Equals(null))
                verticalOffset = DefaultVerticalOffset;
            else
                verticalOffset = -rt.rect.height;




            HintPanel h = Watchman.Get<HintPanel>();
            Vector3 atPosition = eventData.position + new Vector2(0, verticalOffset);
            h.ShowHint(atPosition, Hint);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HintPanel h = Watchman.Get<HintPanel>();
            h.Hide();
        }
    }
}
