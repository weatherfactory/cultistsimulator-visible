using System;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.UI
{
    public class ResolutionSettingControlStrategy : AbstractSettingControlStrategy
    {


        public override void SetSliderValues(Slider slider)
        {

            var availableResolutions = Registry.Get<ScreenResolutionAdapter>().GetAvailableResolutions();
            slider.minValue = 0;
            slider.maxValue = availableResolutions.Count - 1;

            int candidateSliderValue = (int)boundSetting.CurrentValue;
            if(candidateSliderValue == boundSetting.DefaultValue)
            {
                //if the slider value is still the default value (-1 or other distinctive marker) then resolution has never been intentionally set: find a match for the current resolution
                var resolutionIndex = availableResolutions.FindIndex(res =>
                    res.height == Registry.Get<ScreenResolutionAdapter>().GetScreenHeight() && Registry.Get<ScreenResolutionAdapter>().GetScreenWidth() == res.width);
                //original code used screen.height and screen.width; I half-remember I might have needed to do exactly that

                //if we can't find a match, then just pick a candidate from the middle of the range
                if (resolutionIndex == -1)
                    candidateSliderValue = availableResolutions.Count / 2;
            }


            slider.SetValueWithoutNotify(candidateSliderValue);

        }


        public override string GetLabelForValue(float forValue)
        {
            //for value is the infex of the chosen resolution in the list

            var availableResolutions = Registry.Get<ScreenResolutionAdapter>().GetAvailableResolutions();

            var chosenResolution = availableResolutions[(int)forValue];

            string desc = chosenResolution.width + "\n x \n" + chosenResolution.height;
            return desc;
        }
    }
}