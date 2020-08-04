using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.TabletopUi.Scripts.Services
{
    public class NotificationArgs
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool DuplicatesAllowed { get; set; }
        

        public NotificationArgs()
        {
            DuplicatesAllowed = true;
        }
    }

    public class ShowNotificationEvent : UnityEvent<NotificationArgs>
    {

    }


    public class Concursum: MonoBehaviour
    {

        public ShowNotificationEvent ShowNotificationEvent;

        public void Awake()
        {
            var registryAccess=new Registry();

            var storefrontServicesProvider = new StorefrontServicesProvider();
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Steam);
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Gog);
            registryAccess.Register<StorefrontServicesProvider>(storefrontServicesProvider);


            if (ShowNotificationEvent == null)
                ShowNotificationEvent = new ShowNotificationEvent();

            var registry=new Registry();
            registry.Register<Concursum>(this);


        }

        public void ShowNotification(NotificationArgs args)
        {
            ShowNotificationEvent.Invoke(args);
            Debug.Log("called");
        }

    }
}
