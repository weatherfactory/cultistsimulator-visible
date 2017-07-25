using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.CS.TabletopUI
{

    public class Notifier : MonoBehaviour, INotifier
    {
        [SerializeField]
        private Transform windowHolderFixed;
        [SerializeField]
        private Transform notificationHolder;
        [SerializeField]
        private NotificationLog notificationLog;
        [SerializeField]
        private TabletopImageBurner tabletopBurner;

        public void DebugLog(string text)
        {
            Debug.Log(text);
        }
        public void PushTextToLog(string text) {
            notificationLog.AddText(text);
        }

        public void ShowNotificationWindow(string title, string description,float duration=10) {
            var notification = BuildNotificationWindow(duration);
            notification.SetDetails(title, description);
        }

        public void ShowElementDetails(Element element) {
            var detailWindow = BuildElementDetailsWindow();
            detailWindow.SetElementCard(element);
        }

        private ElementDetailsWindow BuildElementDetailsWindow() {
            var window = PrefabFactory.CreateLocally<ElementDetailsWindow>(windowHolderFixed);
            return window;
        }

        public void ShowSlotDetails(SlotSpecification slot) {
            var detailWindow = BuildSlotDetailsWindow();
            detailWindow.SetSlot(slot);
        }

        private SlotDetailsWindow BuildSlotDetailsWindow() {
            var window = PrefabFactory.CreateLocally<SlotDetailsWindow>(windowHolderFixed);
            return window;
        }

        private NotificationWindow BuildNotificationWindow(float duration) {
            var notification = PrefabFactory.CreateLocally<NotificationWindow>(notificationHolder);
            notification.SetDuration(duration);
            return notification;
        }


        public void TokenReturnedToTabletop(DraggableToken draggableToken, INotification reason) {
            if (reason != null)
                ShowNotificationWindow(reason.Title, reason.Description);
        }

        // TabletopImageBurner

        public void ShowImageBurn(string spriteName, DraggableToken token, float duration, float scale, TabletopImageBurner.ImageLayoutConfig alignment) {
            tabletopBurner.ShowImageBurn(spriteName, token, duration, scale, alignment);
        }
    }
}
