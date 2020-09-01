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
        private bool _initialisationComplete=false;

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

            Slider.SetValueWithoutNotify(boundSetting.CurrentValue);


            gameObject.name = "SettingControl_" + boundSetting.Id;

            
            SetValueLabel(boundSetting.CurrentValue);

            _initialisationComplete = true;

        }

        public void OnValueChanged(float newValue)
        {
            //I added this guard clause because otherwise the OnValueChanged event can fire while the slider initial values are being set -
            //for example, if the minvalue is set to > the default control value of 0. This could be fixed by
            //adding the listener in code rather than the inspector, but I'm hewing away from that. It could also be 'fixed' by changing the
            //order of the initialisation steps, but that's half an hour of my time I don't want to lose again next time I fiddle :) - AK
            if(_initialisationComplete)
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
