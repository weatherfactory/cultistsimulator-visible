using System;
using SecretHistories.Constants;
using SecretHistories.UI;
using SecretHistories.Services;

using UnityEngine.UI;

namespace SecretHistories.UI
{
    public class FucineSliderSettingControlStrategy : SliderSettingControlStrategy
    {
        public override void SetSliderValues(Slider slider)
        {
            slider.minValue = boundSetting.MinValue;
            slider.maxValue = boundSetting.MaxValue;
            
            slider.SetValueWithoutNotify( Convert.ToSingle(boundSetting.CurrentValue));
        }


        public override string GetLabelForValue(float forValue)
        {
            boundSetting.ValueLabels.TryGetValue(forValue.ToString(), out var matchingValueLabelString);
            if (!string.IsNullOrEmpty(matchingValueLabelString))
            {
                return matchingValueLabelString;
            }
            else
                return NoonConstants.TEMPLATE_MARKER +  forValue;
        }

    }
}