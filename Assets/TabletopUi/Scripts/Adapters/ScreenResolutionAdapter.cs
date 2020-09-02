using System;
using System.Collections.Generic;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
    public class ScreenResolutionAdapter: MonoBehaviour, ISettingSubscriber
    {
     

        public void UpdateValueFromSetting(float newValue)
        {
            throw new System.NotImplementedException();
        }

        protected void SetResolution(Resolution resolution)
        {
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        public List<Resolution> GetAvailableResolutions()
        {
            return new List<Resolution>(Screen.resolutions);
        }

        public Resolution GetCurrentResolution()
        {
            return Screen.currentResolution;
        }

        public string GetResolutionDescription(Resolution r)
        {
            string desc = r.width + "\n x \n" + r.height;
            return desc;
        }


    }
}