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
using UnityEngine;

namespace Assets.Core.Entities
{
    [FucineImportable("settings")]
    public class Setting : AbstractEntity<Setting>
        { 
        
            [FucineValue]
        public string TabId { get; set; }

        [FucineValue]
        public string Hint { get; set; }

        [FucineValue]
        public string HintLocId { get; set; }

        [FucineValue(DefaultValue = 0)]
        public int MinValue { get; set; }
            
        [FucineValue(DefaultValue=1)]
        public int MaxValue { get; set; }

        [FucineValue]
        public string DefaultValue { get; set; }

        [FucineValue]
        public string DataType { get; set; }


        
        [FucineDict]
        public Dictionary<string,string> ValueLabels { get; set; }

        private object _currentValue;
        public object CurrentValue
        {
            get
            {
                if(DataType==nameof(Single))
                    return Convert.ToSingle(_currentValue);
                if (DataType == nameof(Int32))
                    return Convert.ToInt32(_currentValue);
                if (DataType == nameof(String))
                    return Convert.ToString(_currentValue);
                else
                {
                    NoonUtility.Log("Unknown setting data type: " + DataType,2);
                    return null;
                }
            }
            set
            {
                _currentValue = value;
                foreach (var subscriber in _subscribers)
                {
                    subscriber.UpdateValueFromSetting(CurrentValue);
                }
            }
        }


        public string GetCurrentValueAsHumanReadableString()
        {
            //currently, this caters only for key bindings.
            string currentString = CurrentValue.ToString();
            int lastSlashPosition = currentString.LastIndexOf('/');

            if (lastSlashPosition < 0)
                return currentString;

            string readableString = currentString.Substring(lastSlashPosition + 1);
            return readableString.ToUpperInvariant();
        }


        private readonly List<ISettingSubscriber> _subscribers=new List<ISettingSubscriber>();


        

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
            //if a value for this Setting has been stored in config, set the value accordingly
            //otherwise, set it to the default value
            if (DataType == nameof(Single))
            {
                var potentialValue = Registry.Get<Config>().GetConfigValueAsFloat(Id);
                if (potentialValue == null)
                    CurrentValue = DefaultValue;
                else
                    CurrentValue = potentialValue;
            }
            else if (DataType == nameof(Int32))
            {
                var potentialValue = Registry.Get<Config>().GetConfigValueAsInt(Id);
                if (potentialValue == null)
                    CurrentValue = DefaultValue;
                else
                    CurrentValue = potentialValue;
            }
            else
            {
                var potentialValue= Registry.Get<Config>().GetConfigValueAsString(Id);
                if (string.IsNullOrEmpty(potentialValue))
                    CurrentValue = DefaultValue;
                else
                    CurrentValue = potentialValue;
            }

            //now that the value's been set, create an observer so that the config is updated with any changes to the Setting.
            //(Don't do it before now or we'd needlessly double-update the config)

            SettingObserverForConfig observer =new SettingObserverForConfig(Id,Registry.Get<Config>());

            AddSubscriber(observer);

        }


        public Setting(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }



        public void AddSubscriber(ISettingSubscriber subscriber)
        {
            if(_subscribers.Contains(subscriber))
                NoonUtility.Log($"Trying to add the same subscriber twice to Setting {Id}");
            else
            {
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
