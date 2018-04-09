#pragma warning disable 0649
using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.CS.TabletopUI {

    public class Notifier : MonoBehaviour, INotifier {
        
        [Header("Notification")]
        [SerializeField] Transform notificationHolder;
        [SerializeField] NotificationLog notificationLog;

        [Header("Token Details")]
        [SerializeField] TokenDetailsWindow tokenDetails;
        [SerializeField] AspectDetailsWindow aspectDetails;

        [Header("Image Burner")]
        [SerializeField] private TabletopImageBurner tabletopBurner;

        public void Initialise() {
            tokenDetails.gameObject.SetActive(false); // ensure this is turned off at the start
            aspectDetails.gameObject.SetActive(false);
        }

        // Notifications

        public void PushTextToLog(string text) {
            notificationLog.AddText(text);
        }

        public void ShowNotificationWindow(string title, string description, float duration = 10) {
            var notification = BuildNotificationWindow(duration);
            notification.SetDetails(title, description);
        }

        private NotificationWindow BuildNotificationWindow(float duration) {
            var notification = PrefabFactory.CreateLocally<NotificationWindow>(notificationHolder);
            notification.SetDuration(duration);
            return notification;
        }

        // Token Details

        // Variant to link to token decay
        public void ShowCardElementDetails(Element element, ElementStackToken token) {
            tokenDetails.ShowElementDetails(element, token);
            aspectDetails.Hide();
        }

        public void ShowElementDetails(Element element, bool fromDetailsWindow = false) {
            if (element.IsAspect == false) {
                tokenDetails.ShowElementDetails(element);
                aspectDetails.Hide();
                return;
            }

            // The following only happens for aspects
            aspectDetails.ShowAspectDetails(element, !fromDetailsWindow);

            if (fromDetailsWindow)
                tokenDetails.ResetTimer(); // ensure the token window timer is restored
            else 
                tokenDetails.Hide(); // hide the token window
        }
        
        public void ShowSlotDetails(SlotSpecification slot, bool highlightGreedy, bool highlightConsumes) {
            tokenDetails.ShowSlotDetails(slot);
            tokenDetails.HighlightSlotIcon(highlightGreedy, highlightConsumes);
            aspectDetails.Hide();
        }

        public void ShowDeckDetails(string deckId, int quantity) {
            Debug.Log("Doing nothing yet");
        }

        // TabletopImageBurner

        public void ShowImageBurn(string spriteName, DraggableToken token, float duration, float scale, TabletopImageBurner.ImageLayoutConfig alignment) {
            tabletopBurner.ShowImageBurn(spriteName, token, duration, scale, alignment);
        }
    }
}
