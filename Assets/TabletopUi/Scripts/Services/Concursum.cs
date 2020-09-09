using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
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

    public class ChangeSettingArgs
    {
        public string Key { get; set; }
        public object Value { get; set; }

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

    public class SettingChangedEvent: UnityEvent<ChangeSettingArgs>
    {}


    public class Concursum:MonoBehaviour
    {

        //things I really really truly want to be global:
        //storefront access
        //notification events
        public ShowNotificationEvent ShowNotificationEvent = new ShowNotificationEvent();
        public ModOperationEvent ModOperationEvent=new ModOperationEvent();
        public ContentUpdatedEvent ContentUpdatedEvent = new ContentUpdatedEvent();
        public CultureChangedEvent CultureChangedEvent =new CultureChangedEvent();
        public SettingChangedEvent SettingChangedEvent = new SettingChangedEvent();

        



        public void SetNewCulture(Culture culture)
        {
            Registry.Get<Config>().CultureId = culture.Id;
            PlayerPrefs.SetString(NoonConstants.CULTURE_SETTING_KEY, value: culture.Id);

            CultureChangedEvent.Invoke(new CultureChangedArgs{NewCulture = culture});
        }


        public void ShowNotification(NotificationArgs args)
        {
            ShowNotificationEvent.Invoke(args);

        }

        public void ModOperation(ModOperationArgs args)
        {
            ModOperationEvent.Invoke(args);
        }

        public void ChangeSetting(ChangeSettingArgs args)
        {
            Registry.Get<Config>().PersistSettingValue(args);
            SettingChangedEvent.Invoke(args);
        }



}
}
