#pragma warning disable 0649
using System;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Abstract;
using SecretHistories.Core;
using SecretHistories.Interfaces;
using SecretHistories.Services;
using UnityEngine;
using SecretHistories.Entities;
using SecretHistories.Constants.Events;
using SecretHistories.Enums;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SecretHistories.UI {



    public class Notifier : MonoBehaviour, INotifier {
        
        [Header("Notification")]
        [SerializeField] Transform notificationHolder;
        [SerializeField] NotificationLog notificationLog;

        [Header("Token Details")]
        [SerializeField] TokenDetailsWindow tokenDetails;
        [SerializeField] AspectDetailsWindow aspectDetails;

        [Header("Image Burner")]
        [SerializeField] private TabletopImageBurner tabletopBurner;

        public void Awake()
        {
            var r=new Watchman();
            r.Register<INotifier>(this);
        }


        public void Start() {
            tokenDetails.gameObject.SetActive(false); // ensure this is turned off at the start
            aspectDetails.gameObject.SetActive(false);

            Watchman.Get<Concursum>().ShowNotificationEvent.AddListener(ShowNotificationWindow);

        }

        // Notifications


		// Text Log Disabled
        public void PushTextToLog(string text) {
        	notificationLog.AddText(text);
        }

        public void ShowNotificationWindow(NotificationArgs args)
        {

            ShowNotificationWindow(args.Title,args.Description,args.DuplicatesAllowed);
        }

        public void ShowNotificationWindow(string title, string description, bool duplicatesAllowed = true) {
       
            float duration = (title.Length + description.Length) / 5; //average reading speed in English is c. 15 characters a second
       
            
            // If no duplicates are allowed, then find any duplicates and hide them
            if (!duplicatesAllowed)
            {
	            foreach (var window in GetDuplicateNotificationWindow(title, description))
		            window.Hide();
            }
            
            var notification = BuildNotificationWindow(duration);
            notification.SetDetails(title, description);
			notification.Show();
        }

        private NotificationWindow BuildNotificationWindow(float duration) {
            var notification = Watchman.Get<PrefabFactory>().CreateLocally<NotificationWindow>(notificationHolder);
            notification.SetDuration(duration);
            return notification;
        }

        private IEnumerable<NotificationWindow> GetDuplicateNotificationWindow(string title, string description)
        {
	        return notificationHolder
		        .GetComponentsInChildren<NotificationWindow>()
		        .Where(window => window.Title == title && window.Description == description);
        }

        // Token Details

        // Variant to link to token decay
        public void ShowCardElementDetails(Element element, ElementStack stack)
		{
            tokenDetails.ShowElementDetails(element, stack);
            aspectDetails.Hide();
        }

        public void ShowElementDetails(Element element, bool fromDetailsWindow = false) {
            if (element.IsAspect == false) {
                tokenDetails.ShowElementDetails(element,null);
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
        
        public void ShowSlotDetails(SphereSpec slot, bool highlightGreedy, bool highlightConsumes) {
            tokenDetails.ShowSlotDetails(slot);
            tokenDetails.HighlightSlotIcon(highlightGreedy, highlightConsumes);
            aspectDetails.Hide();
        }

        public void ShowDeckDetails(DeckSpec deckSpec, int quantity) {
            tokenDetails.ShowDeckDetails(deckSpec, quantity);
            aspectDetails.Hide();
        }

        public void HideDetails()
		{
            tokenDetails.Hide();
            aspectDetails.Hide();
        }



    }
}
