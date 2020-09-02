using System;
using System.Collections.Generic;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
    public class ScreenResolutionAdapter: MonoBehaviour, ISettingSubscriber
    {

        public void Awake()
        {
            var registry=new Registry();
            registry.Register(this);

        }

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



    }
}