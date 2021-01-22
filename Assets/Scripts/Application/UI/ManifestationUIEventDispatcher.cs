using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Application.UI
{
    [RequireComponent(typeof(CanvasGroup))]
  public class ManifestationUIEventDispatcher: MonoBehaviour, IPointerClickHandler
    {

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("fired click on " + gameObject.name + " and then ran ExecuteHierarchy");
            ExecuteEvents.ExecuteHierarchy<IPointerClickHandler>(transform.parent.gameObject, eventData,
                (x, y) => x.OnPointerClick(eventData));
        }
    }
}
