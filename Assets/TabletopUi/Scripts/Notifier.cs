using System;
using System.Collections.Generic;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.CS.TabletopUI
{
    
    public class Notifier : MonoBehaviour,ITokenSubscriber {
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

        public void ShowElementDetails(ElementStack stack)
        {
            var detailWindow = BuildElementDetailsWindow();
            detailWindow.SetElementCard(stack);
        }


        public ElementDetailsWindow BuildElementDetailsWindow()
        {
            var window = PrefabFactory.CreateLocally<ElementDetailsWindow>(windowHolderFixed);
            return window;
        }

        public Notification BuildNotification()
        {
            var notification = PrefabFactory.CreateLocally<Notification>(notificationHolder);
            return notification;
        }


        public void TokenPickedUp(DraggableToken draggableToken)
        {
  
        }

        public void TokenInteracted(DraggableToken draggableToken)
        {
            ElementStack stack = draggableToken as ElementStack;
            if (stack != null)
            {
                    ShowElementDetails(stack);
            }
        }
    }
}
