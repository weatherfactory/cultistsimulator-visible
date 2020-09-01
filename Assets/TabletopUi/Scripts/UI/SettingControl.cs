using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
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
            if(settingToBind==null)
            {
                NoonUtility.Log("Missing setting entity: " + NoonConstants.MUSICVOLUME);
                return;
            }

            boundSetting = settingToBind;

            SliderHint.text = boundSetting.Hint;
            Slider.minValue = boundSetting.MinValue;
            Slider.maxValue = boundSetting.MaxValue;
            gameObject.name = "SettingControl_" + boundSetting.Id;

            Slider.SetValueWithoutNotify(boundSetting.CurrentValue);
            
            SetValueLabel(boundSetting.CurrentValue);



        }

        public void OnValueChanged(float newValue)
        {
            SoundManager.PlaySfx("UISliderMove");
            SetValueLabel(newValue);

            ChangeSettingArgs args = new ChangeSettingArgs
            {
                Key = boundSetting.Id,
                Value = newValue
            };

            Registry.Get<Concursum>().ChangeSetting(args);
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
