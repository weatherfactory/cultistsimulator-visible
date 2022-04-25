using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.UI;
using SecretHistories.Services;

using UnityEngine;

namespace SecretHistories.Entities
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

        //same thing as ValueLabels, but for logic, not UI purposes
        //the inner label of the current value is retrieved via GetInnerLabelForValue()
        [FucineDict]
        public Dictionary<string, string> ValueInnerLabels { get; set; }
        //ie. something like { "id": "CURRENT GAME", "valueInnerLabels": { "0": "cs", "1": "bh" } }

        [FucineValue]
        public string TargetConfigArray { get; set; }

        private readonly HashSet<ISettingSubscriber> _subscribers = new HashSet<ISettingSubscriber>();


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
                object oldValue = _currentValue;
                _currentValue = value;
                foreach (var subscriber in _subscribers)
                {
                    subscriber.BeforeSettingUpdated(oldValue);
                    subscriber.WhenSettingUpdated(CurrentValue);
                }
            }
        }

        public string GetInnerLabelForValue(object value)
        {
            string valueString = value.ToString();
            if (ValueInnerLabels.ContainsKey(valueString))
                return ValueInnerLabels[valueString];
            else
                return valueString;
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



        

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            if(Watchman.Exists<Config>())
                LinkToConfig();
        }

        private void LinkToConfig()
        {
            //if a value for this Setting has been stored in config, set the value accordingly
            //otherwise, set it to the default value
            if (DataType == nameof(Single))
            {
                var potentialValue = Watchman.Get<Config>().GetConfigValueAsFloat(Id);
                if (potentialValue == null)
                    CurrentValue = DefaultValue;
                else
                    CurrentValue = potentialValue;
            }
            else if (DataType == nameof(Int32))
            {
                var potentialValue = Watchman.Get<Config>().GetConfigValueAsInt(Id);
                if (potentialValue == null)
                    CurrentValue = DefaultValue;
                else
                    CurrentValue = potentialValue;
            }
            else
            {
                var potentialValue = Watchman.Get<Config>().GetConfigValueAsString(Id);
                if (string.IsNullOrEmpty(potentialValue))
                    CurrentValue = DefaultValue;
                else
                    CurrentValue = potentialValue;
            }

            //now that the value's been set, create an observer so that the config is updated with any changes to the Setting.
            //(Don't do it before now or we'd needlessly double-update the config)
            ISettingSubscriber observer;
            if (TargetConfigArray == null)
                observer = new SettingObserverForConfig(Id, Watchman.Get<Config>());
            else
                //settings that have TargetConfigArray defined will not have its own line in the .ini, but will contribute to that TargetConfigArrayValue
                //when the settings changes, its current value is added to the target array
                //(value is taken from GetCurrentValueAsHumanReadableString() - so we can make use of ValueInnerLabels)
                observer = new SettingArrayObserverForConfig(this, Watchman.Get<Config>());

            AddSubscriber(observer);
        }


        public Setting(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }



        public bool AddSubscriber(ISettingSubscriber subscriber)
        {
               return _subscribers.Add(subscriber);
        }

        public bool RemoveSubscriber(ISettingSubscriber subscriber)
        {
            return _subscribers.Remove(subscriber);
        }

    }





}
