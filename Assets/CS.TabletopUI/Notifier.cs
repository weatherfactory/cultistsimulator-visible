using System;
using System.Collections.Generic;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.CS.TabletopUI
{
    
    public class Notifier : MonoBehaviour,IElementCardSubscriber {
        [Header("Prefabs")]
        [SerializeField]
        ElementDetailsWindow elementDetailWindowPrefab;

        [SerializeField]  Notification notificationPrefab;
        [SerializeField]
        Transform windowHolderFixed;

        private int _maxNumElementWindows;
        private List<ElementDetailsWindow> elementWindows = new List<ElementDetailsWindow>();

        public Notifier(int maxNumElementWindows)
        {
            _maxNumElementWindows = maxNumElementWindows;
        }


        public void ShowElementDetails(ElementCard card)
        {

            var notification = BuildNotification();
            notification.SetDetails("foo","bar");
            return;

            if (_maxNumElementWindows > 0 && elementWindows.Count == _maxNumElementWindows)
                HideElementDetails(elementWindows[0].GetElementCard());

            //		PutTokenInAir(card.transform as RectTransform);
            var window = BuildElementDetailsWindow(0);
            //		window.transform.position = card.transform.position;
            window.SetElementCard(card);
            elementWindows.Add(window);
        }

       public void HideElementDetails(ElementCard card)
        {
            //if (Draggable.itemBeingDragged == null || Draggable.itemBeingDragged.gameObject != card.gameObject)
            //	PutTokenOnTable(card.transform as RectTransform); // remove card from details window before hiding it, so it isn't removed, if we're not already dragging it

            elementWindows.Remove(card.detailsWindow);
            card.detailsWindow.Hide();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration">Fade element out in this many seconds; 0 persists until removed</param>
        /// <returns></returns>
        public ElementDetailsWindow BuildElementDetailsWindow(int duration)
        {
            var window = Instantiate(elementDetailWindowPrefab) as ElementDetailsWindow;
            window.transform.SetParent(windowHolderFixed);
            window.transform.localPosition = Vector3.zero;
            window.transform.localScale = Vector3.one;
            window.transform.localRotation = Quaternion.identity;
            return window;
        }

        public Notification BuildNotification()
        {

            var notification = Instantiate(this.notificationPrefab) as Notification;
            notification.transform.SetParent(windowHolderFixed);
            notification.transform.localPosition = Vector3.zero;
            notification.transform.localScale = Vector3.one;
            notification.transform.localRotation = Quaternion.identity;
            return notification;
            
        }



        public void ElementPickedUp(ElementCard elementCard)
        {
            throw new System.NotImplementedException();
        }

        public void HideAllElementDetails()
        {
            for (int i = 0; i < elementWindows.Count; i++)
                HideElementDetails(elementWindows[i].GetElementCard());
        }
    }
}
