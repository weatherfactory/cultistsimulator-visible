#pragma warning disable 0649
using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;
using Assets.Core.Entities;

namespace Assets.CS.TabletopUI {

    public class Notifier : MonoBehaviour, INotifier {
        
        [Header("Notification")]
        [SerializeField] Transform notificationHolder;
        [SerializeField] NotificationLog notificationLog;
		[SerializeField] NotificationWindow saveErrorWindow;
		[SerializeField] NotificationWindow saveDeniedWindow;

        [Header("Token Details")]
        [SerializeField] TokenDetailsWindow tokenDetails;
        [SerializeField] AspectDetailsWindow aspectDetails;

        [Header("Image Burner")]
        [SerializeField] private TabletopImageBurner tabletopBurner;

        public void Initialise() {
            tokenDetails.gameObject.SetActive(false); // ensure this is turned off at the start
            aspectDetails.gameObject.SetActive(false);
			saveErrorWindow.gameObject.SetActive(false);
			saveDeniedWindow.gameObject.SetActive(false);
        }

        // Notifications


		// Text Log Disabled
        public void PushTextToLog(string text) {
        	notificationLog.AddText(text);
        }
        

        public void ShowNotificationWindow(string title, string description) {
       
            float duration = (title.Length + description.Length) / 7; //average reading speed in English is c. 15 characters a second
            
            var notification = BuildNotificationWindow(duration);
            notification.SetDetails(title, description);
			notification.Show();
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

        public void ShowDeckDetails(IDeckSpec deckSpec, int quantity) {
            tokenDetails.ShowDeckDetails(deckSpec, quantity);
            aspectDetails.Hide();
        }

        public void HideDetails()
		{
            tokenDetails.Hide();
            aspectDetails.Hide();
        }

		public void ShowSaveError( bool on )
		{
			if (TabletopManager.IsInMansus())
			{
				if (saveDeniedWindow == null)
					return;

				if (on)
				{
					saveDeniedWindow.Show();
				}
				else
				{
					saveDeniedWindow.HideNoDestroy();
				}
			}
			else
			{
				if (saveErrorWindow == null)
					return;

				if (on)
				{
					saveErrorWindow.Show();
				}
				else
				{
					saveErrorWindow.HideNoDestroy();
				}
			}
		}


        // TabletopImageBurner

        public void ShowImageBurn(string spriteName, DraggableToken token, float duration, float scale, TabletopImageBurner.ImageLayoutConfig alignment) {
            tabletopBurner.ShowImageBurn(spriteName, token, duration, scale, alignment);
        }
    }
}
