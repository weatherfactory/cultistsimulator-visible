using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Constants.Modding;

using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SecretHistories.Services
{
    public class ButtonCommand
    {
        public string Caption { get; set; }

    }

    public class NotificationArgs
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool DuplicatesAllowed { get; set; }
        public List<ButtonCommand> Buttons { get; set; }=new List<ButtonCommand>();
        

        public NotificationArgs()
        {
            DuplicatesAllowed = true;
        }

        public NotificationArgs(string title,string description):base()
        {
            Title = title;
            Description = description;
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

    public class CultureChangedArgs
    {

        public Culture NewCulture { get; set; }

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

    public class CultureChangedEvent : UnityEvent<CultureChangedArgs>
    {

    }


    public class Concursum:MonoBehaviour
    {
        //THIS CLASS LOOKS LIKE PETER VAUGHAN IN FIERCE MODE

        //things I really really truly want to be global:
        //storefront access
        //notification events
        //log
        public ShowNotificationEvent ShowNotificationEvent = new ShowNotificationEvent();
        public ModOperationEvent ModOperationEvent=new ModOperationEvent();
        public ContentUpdatedEvent ContentUpdatedEvent = new ContentUpdatedEvent();
        public CultureChangedEvent BeforeChangingCulture = new CultureChangedEvent();
        public CultureChangedEvent ChangingCulture =new CultureChangedEvent();
        public CultureChangedEvent AfterChangingCulture = new CultureChangedEvent();


#pragma warning disable 649
        [SerializeField] private SecretHistory secretHistory;
#pragma warning restore 649


        public void SetNewCulture(Culture culture)
        {
            Registry.Get<Config>().PersistConfigValue(NoonConstants.CULTURE_SETTING_KEY,culture.Id);

            BeforeChangingCulture.Invoke(new CultureChangedArgs { NewCulture = culture });
            ChangingCulture.Invoke(new CultureChangedArgs{NewCulture = culture});
            AfterChangingCulture.Invoke(new CultureChangedArgs { NewCulture = culture });

            NoonUtility.Log($"Changed culture to {culture.Id}");
        }


        public void ToggleSecretHistory()
        {
            secretHistory.SetVisible(!secretHistory.IsVisible);
        }

        public void ShowNotification(NotificationArgs args)
        {
            ShowNotificationEvent.Invoke(args);

        }

        public void ContentUpdated(ContentUpdatedArgs args)
        {
            ContentUpdatedEvent.Invoke(args);

        }

        public void ModOperation(ModOperationArgs args)
        {
            ModOperationEvent.Invoke(args);
        }




}
}
