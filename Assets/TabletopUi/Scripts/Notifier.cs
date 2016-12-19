using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.CS.TabletopUI
{
    
    public class Notifier : MonoBehaviour {
        [SerializeField]
        private Transform windowHolderFixed;
        [SerializeField]
        private Transform notificationHolder;

        public void ShowNotificationWindow(string title, string description)
        {
            var notification = BuildNotificationWindow();
            notification.SetDetails(title, description);
        }

        public void ShowElementDetails(Element element)
        {
            var detailWindow = BuildElementDetailsWindow();
            detailWindow.SetElementCard(element);
        }

        private ElementDetailsWindow BuildElementDetailsWindow()
        {
            var window = PrefabFactory.CreateLocally<ElementDetailsWindow>(windowHolderFixed);
            return window;
        }


        private SlotDetailsWindow BuildSlotDetailsWindow() {
            var window = PrefabFactory.CreateLocally<SlotDetailsWindow>(windowHolderFixed);
            return window;
        }

        private NotificationWindow BuildNotificationWindow()
        {
            var notification = PrefabFactory.CreateLocally<NotificationWindow>(notificationHolder);
            return notification;
        }


        public void TokenReturnedToTabletop(DraggableToken draggableToken, INotification reason)
        {
            if(reason!=null)
            ShowNotificationWindow(reason.Title,reason.Description);
        }
    }
}
