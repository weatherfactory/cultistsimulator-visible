using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.UI
{
    public abstract class AbstractSettingControlStrategy
    {
        protected Setting boundSetting;

        public void Initialise(Setting settingToBind)
        {
            boundSetting = settingToBind;
        }

        
        public string SettingId
        {
            get { return boundSetting.Id; }
        }
        public string SettingHint
        {
            get { return boundSetting.Hint; }
        }

        public float SettingCurrentValue
        {
            get { return boundSetting.CurrentValue; }
        }


        public abstract void SetSliderValues(Slider slider);

        protected void PersistSettingValue(float newValue)
        {
            ChangeSettingArgs args = new ChangeSettingArgs
            {
                Key = boundSetting.Id,
                Value = newValue
            };

            Registry.Get<Concursum>().ChangeSetting(args);
        }

        public string ChangeSettingValueAndGetLabel(float newValue)
        {
            PersistSettingValue(newValue);

            return GetLabelForValue(newValue);
        }

        public string GetLabelForCurrentValue()
        {
            return GetLabelForValue(boundSetting.CurrentValue);
        }

        protected abstract string GetLabelForValue(float forValue);
    
    }
}