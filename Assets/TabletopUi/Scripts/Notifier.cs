using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
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

        public void ShowNotificationWindow(string title, string description)
        {
            var notification = BuildNotificationWindow();
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

        public NotificationWindow BuildNotificationWindow()
        {
            var notification = PrefabFactory.CreateLocally<NotificationWindow>(notificationHolder);
            return notification;
        }


        public void TokenEffectCommandSent(DraggableToken draggableToken, IEffectCommand effectCommand)
        {
            ShowNotificationWindow(effectCommand.Title,effectCommand.Description);
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

        public void TokenReturnedToTabletop(DraggableToken draggableToken, INotification reason)
        {
            if(reason!=null)
            ShowNotificationWindow(reason.Title,reason.Description);
        }
    }
}
