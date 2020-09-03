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

            
            var resolutionIndex = availableResolutions.FindIndex(res =>
                res.height == Registry.Get<ScreenResolutionAdapter>().GetScreenHeight() && Registry.Get<ScreenResolutionAdapter>().GetScreenWidth() == res.width);
            //original code used screen.height and screen.width; I half-remember I might have needed to do exactly that


            if (resolutionIndex == -1)
                resolutionIndex = availableResolutions.Count / 2;
            
            slider.minValue = 0;
            slider.maxValue = availableResolutions.Count - 1;
            
            slider.SetValueWithoutNotify(resolutionIndex);

            if(Math.Abs(resolutionIndex - boundSetting.CurrentValue) > 0.5) //cos floating points
                PersistSettingValue(resolutionIndex); //if we just calculated the resolution index from the current resolution,
            //rather than retrieving it from settings, we need to persist it.

        }


        protected override string GetLabelForValue(float forValue)
        {
            //for value is the infex of the chosen resolution in the list

            var availableResolutions = Registry.Get<ScreenResolutionAdapter>().GetAvailableResolutions();

            var chosenResolution = availableResolutions[(int)forValue];

            string desc = chosenResolution.width + "\n x \n" + chosenResolution.height;
            return desc;
        }
    }
}