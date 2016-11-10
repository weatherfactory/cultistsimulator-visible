using System;
using System.Collections.Generic;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.CS.TabletopUI
{
    
    public class Notifier : MonoBehaviour,ITokenSubscriber {
        [Header("Prefabs")]
        [SerializeField]
        ElementDetailsWindow elementDetailsWindowPrefab;

        [SerializeField]  Notification notificationPrefab;
        [SerializeField]
        private Transform windowHolderFixed;
        [SerializeField]
        private Transform notificationHolder;
        [SerializeField]
        private int maxNumElementWindows;


        public void ShowNotification(string title, string description)
        {
            var notification = BuildNotification();
            notification.SetDetails(title, description);
        }

        public void ShowElementDetails(ElementCard card)
        {
            var detailWindow = BuildElementDetailsWindow();
            detailWindow.SetElementCard(card);
        }


        public ElementDetailsWindow BuildElementDetailsWindow()
        {
            var window = Instantiate(elementDetailsWindowPrefab);
            window.transform.SetParent(windowHolderFixed);
            window.transform.localPosition = Vector3.zero;
            window.transform.localScale = Vector3.one;
            window.transform.localRotation = Quaternion.identity;
            return window;
        }

        public Notification BuildNotification()
        {

            var notification = Instantiate(this.notificationPrefab) as Notification;
            notification.transform.SetParent(notificationHolder);
            notification.transform.localPosition = Vector3.zero;
            notification.transform.localScale = Vector3.one;
            notification.transform.localRotation = Quaternion.identity;
            return notification;
            
        }


        public void TokenPickedUp(DraggableToken draggableToken)
        {
  
        }

        public void TokenInteracted(DraggableToken draggableToken)
        {
            ElementCard card = draggableToken as ElementCard;
            if (card != null)
            {
                    ShowElementDetails(card);
            }
        }
    }
}
