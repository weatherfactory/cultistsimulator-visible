using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Noon;
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

    public class ModOperationArgs
    {
        public Mod Mod { get; set; }
        public string PublishedFileId { get; set; }
        public bool Successful { get; set; }
        public string Message { get; set; }
        
    }

    public class ContentUpdatedArgs
    {

        public string Message { get; set; }

    }

    public class ShowNotificationEvent : UnityEvent<NotificationArgs>
    {

    }

    public class ModOperationEvent : UnityEvent<ModOperationArgs>
    {

    }

    public class ContentUpdatedEvent : UnityEvent<ContentUpdatedArgs>
    {

    }


    public class Concursum : MonoBehaviour
    {
        [SerializeField] public LanguageManager languageManager;

        //things I really really truly want to be global:
        //storefront access
        //notification events
        public ShowNotificationEvent ShowNotificationEvent;
        public ModOperationEvent ModOperationEvent;
        public ContentUpdatedEvent ContentUpdatedEvent;


        public void Awake()
        {
            var registryAccess = new Registry();

            var storefrontServicesProvider = new StorefrontServicesProvider();
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Steam);
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Gog);
            registryAccess.Register<StorefrontServicesProvider>(storefrontServicesProvider);
            


            if (ShowNotificationEvent == null)
                ShowNotificationEvent = new ShowNotificationEvent();

            if (ModOperationEvent == null)
                ModOperationEvent = new ModOperationEvent();


            if (ContentUpdatedEvent == null)
                ContentUpdatedEvent = new ContentUpdatedEvent();


            registryAccess.Register<Concursum>(this);

            //TODO: make this async
            registryAccess.Register(new ModManager());
            registryAccess.Register<ICompendium>(new Compendium());

            var contentImporter = new CompendiumLoader();
            var log = contentImporter.PopulateCompendium(Registry.Retrieve<ICompendium>());

        foreach(var m in log.GetMessages())
            NoonUtility.Log(m);


        languageManager.Initialise();
        registryAccess.Register<LanguageManager>(languageManager);

        }


        public void ShowNotification(NotificationArgs args)
        {
            ShowNotificationEvent.Invoke(args);

        }

        public void ModOperation(ModOperationArgs args)
        {
            ModOperationEvent.Invoke(args);
        }

        public void ContentUpdated(ContentUpdatedArgs args)
        {
            ContentUpdatedEvent.Invoke(args);
        }

}
}
