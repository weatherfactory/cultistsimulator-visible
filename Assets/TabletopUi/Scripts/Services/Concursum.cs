using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
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

    public class ModUploadedArgs
    {
        public Mod Mod { get; set; }
        public string PublishedFileId { get; set; }
        
    }

    public class ShowNotificationEvent : UnityEvent<NotificationArgs>
    {

    }

    public class ModUploadedEvent : UnityEvent<ModUploadedArgs>
    {

    }


    public class Concursum : MonoBehaviour
    {
        //things I really really truly want to be global:
        //storefront access
        //notification events
        public ShowNotificationEvent ShowNotificationEvent;
        public ModUploadedEvent ModUploadedEvent;


        public void Awake()
        {
            var registryAccess = new Registry();

            var storefrontServicesProvider = new StorefrontServicesProvider();
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Steam);
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Gog);
            registryAccess.Register<StorefrontServicesProvider>(storefrontServicesProvider);


            if (ShowNotificationEvent == null)
                ShowNotificationEvent = new ShowNotificationEvent();

            if (ModUploadedEvent == null)
                ModUploadedEvent = new ModUploadedEvent();

            var registry = new Registry();
            registry.Register<Concursum>(this);


        }

        public void ShowNotification(NotificationArgs args)
        {
            ShowNotificationEvent.Invoke(args);

        }

        public void ModUploaded(ModUploadedArgs args)
        {
            ModUploadedEvent.Invoke(args);
        }

}
}
