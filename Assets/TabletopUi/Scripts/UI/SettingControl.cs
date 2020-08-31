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
            //  Slider.value = setting.DefaultValue;
            //Slider = setting.DefaultValue;
        }

        public void OnValueChanged(float newValue)
        {
            NoonUtility.Log(newValue);
        }
    }
}
