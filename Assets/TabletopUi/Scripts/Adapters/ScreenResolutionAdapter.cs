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

        /// <summary>
        /// we use Screen.height rather than CurrentResolution.height because Screen.height respects the resolution set in the Editor
        /// </summary>
        /// <returns></returns>
        public int GetScreenHeight()
        {
            return Screen.height;
        }
        /// <summary>
        /// we use Screen.width rather than CurrentResolution.width because Screen.height respects the resolution set in the Editor
        /// </summary>
        /// <returns></returns>
        public int GetScreenWidth()
        {
            return Screen.width;
        }



    }
}