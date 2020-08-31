using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.UI
{
    public class SettingControl: MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI SliderHint;
        [SerializeField]
        private Slider Slider;
        [SerializeField]
        private TextMeshProUGUI SliderValueLabel;

        private Setting boundSetting;

        public void Initialise(Setting settingToBind)
        {
            boundSetting = settingToBind;

            SliderHint.text = boundSetting.Hint;
            Slider.minValue = boundSetting.MinValue;
            Slider.maxValue = boundSetting.MaxValue;
            gameObject.name = "SettingControl_" + boundSetting.Id;
            Slider.SetValueWithoutNotify(boundSetting.DefaultValue);

            SetValueLabel(boundSetting.DefaultValue);
        }

        public void OnValueChanged(float newValue)
        {
            SoundManager.PlaySfx("UISliderMove");
            SetValueLabel(newValue);
        }

        private void SetValueLabel(float forValue)
        {
            boundSetting.ValueLabels.TryGetValue(forValue.ToString(), out var matchingValueLabelString);
            if (!string.IsNullOrEmpty(matchingValueLabelString))
                SliderValueLabel.text = matchingValueLabelString;
            else
                SliderValueLabel.text = forValue.ToString();
        }
    }
}
