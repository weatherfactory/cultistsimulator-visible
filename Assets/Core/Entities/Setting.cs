using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;

namespace Assets.Core.Entities
{
    [FucineImportable("settings")]
    public class Setting : AbstractEntity<Setting>
        { 
        
            [FucineValue]
        public string Tab { get; set; }

        [FucineValue]
        public string Hint { get; set; }

        [FucineValue]
        public string HintLocId { get; set; }

        [FucineValue]
        public int MinValue { get; set; }
            
        [FucineValue]
        public int MaxValue { get; set; }

        [FucineValue]
        public int DefaultValue { get; set; }

        [FucineDict]
        public Dictionary<string,string> ValueLabels { get; set; }

        public float CurrentValue { get; private set; }


        private List<ISettingSubscriber> _subscribers=new List<ISettingSubscriber>();


        

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
            Registry.Get<Concursum>().SettingChangedEvent.AddListener(OnSettingChangedEvent);

            var potentialValue= Registry.Get<Config>().GetPersistedSettingValue(Id);
           if (potentialValue == null)
               CurrentValue = DefaultValue;
           else
               CurrentValue = (float) potentialValue;
        }


        public Setting(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

        public void OnSettingChangedEvent(ChangeSettingArgs args)
        {
            if (args.Key == Id)
            {
                CurrentValue = args.Value;
                foreach (var subscriber in _subscribers)
                {
                    subscriber.UpdateValueFromSetting(CurrentValue);
                }
            }
        }

        public void AddSubscriber(ISettingSubscriber subscriber)
        {
            if(_subscribers.Contains(subscriber))
                NoonUtility.Log($"Trying to add the same subscriber twice to Setting {Id}");
            else
            {
                subscriber.UpdateValueFromSetting(CurrentValue);
                _subscribers.Add(subscriber);
            }
        }

        public void RemoveSubscriber(ISettingSubscriber subscriber)
        {
            if (!_subscribers.Contains(subscriber))
                NoonUtility.Log($"Trying to remove a nonexistent subscriber from Setting {Id}");
            else
                _subscribers.Remove(subscriber);
        }

    }





}
