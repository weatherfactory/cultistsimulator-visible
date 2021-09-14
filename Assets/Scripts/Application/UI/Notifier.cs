#pragma warning disable 0649
using System;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Abstract;
using SecretHistories.Core;
using SecretHistories.Fucine;
using SecretHistories.Services;
using UnityEngine;
using SecretHistories.Entities;
using SecretHistories.Constants.Events;
using SecretHistories.Elements;
using SecretHistories.Enums;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SecretHistories.UI {

    public enum CustomNotificationWindowId
    {
        ShowSaveError = 1
    };

    public class CustomNotificationWindowArgs
    {
        public CustomNotificationWindowId WindowId { get; set; }
        public string AdditionalText { get; set; }
    }

    public class Notifier : MonoBehaviour {
        
        [Header("Notification")]
        [SerializeField] Transform notificationHolder;

        [Header("Token Details")]
        [SerializeField] TokenDetailsWindow tokenDetails;
        [SerializeField] SlotDetailsWindow slotDetails;
        [SerializeField] AspectDetailsWindow aspectDetails;

        [SerializeField] NotificationWindow SaveErrorWindow;
        [SerializeField] NotificationWindow SaveDeniedWindow;

        public void Start() {
            tokenDetails.gameObject.SetActive(false); // ensure this is turned off at the start
            aspectDetails.gameObject.SetActive(false);

            Watchman.Get<Concursum>().ShowNotificationEvent.AddListener(ShowNotificationWindow);

            var r = new Watchman();
            r.Register<Notifier>(this);
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
            slotDetails.Hide();
        }

        public void ShowElementDetails(Element element) {
            
                aspectDetails.ShowAspectDetails(element, true);
                tokenDetails.ResetTimer(); 
                slotDetails.ResetTimer();

        }
        
        public void ShowSlotDetails(SphereSpec slotSpec) {
            slotDetails.ShowSlotDetails(slotSpec);
            tokenDetails.Hide();
            aspectDetails.Hide();
        }

        public void ShowDeckDetails(DeckSpec deckSpec, int quantity) {
            tokenDetails.ShowDeckDetails(deckSpec, quantity);
            aspectDetails.Hide();
            slotDetails.Hide();
        }

        public void HideDetails()
		{
            tokenDetails.Hide();
            aspectDetails.Hide();
            slotDetails.Hide();
        }

        public void ShowCustomWindow(CustomNotificationWindowArgs args)
        {
            if(args.WindowId==CustomNotificationWindowId.ShowSaveError)
            {
                SaveErrorWindow.SetAdditionalText(args.AdditionalText);
                SaveErrorWindow.Show();
            }
            else
               NoonUtility.Log($"Unknown notification window id: {args.WindowId}");
        }



        

    }
}
