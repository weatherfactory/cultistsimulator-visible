using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.UI
{
    public class FucineSettingControlStrategy : AbstractSettingControlStrategy
    {
        public override void SetSliderValues(Slider slider)
        {

            slider.minValue = boundSetting.MinValue;
            slider.maxValue = boundSetting.MaxValue;
            slider.SetValueWithoutNotify(boundSetting.CurrentValue);
        }


        protected override string GetLabelForValue(float forValue)
        {
            boundSetting.ValueLabels.TryGetValue(forValue.ToString(), out var matchingValueLabelString);
            if (!string.IsNullOrEmpty(matchingValueLabelString))
                return matchingValueLabelString;
            else
                return forValue.ToString();
        }

    }
}