using Assets.Core.Entities;
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

        public abstract string ChangeValue(float newValue);

        public string GetLabelForCurrentValue()
        {
            return GetLabelForValue(boundSetting.CurrentValue);
        }

        protected abstract string GetLabelForValue(float forValue);
    
    }
}