using System;
using System.Collections.Generic;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Interfaces;

using UnityEngine;

namespace SecretHistories.Services
{
    public class ScreenResolutionAdapter: MonoBehaviour, ISettingSubscriber
    {

        public void Awake()
        {
            var registry=new Registry();
            registry.Register(this);

        }

        public void Initialise()
        {
            var registry = new Registry();
            registry.Register(this);


            var resolutionSetting = Registry.Get<Compendium>().GetEntityById<Setting>(NoonConstants.RESOLUTION);
            if (resolutionSetting != null)
            {
                resolutionSetting.AddSubscriber(this);
                WhenSettingUpdated(resolutionSetting.CurrentValue);
            }
            else
                NoonUtility.Log("Missing setting entity: " + NoonConstants.RESOLUTION);


        }

        public void WhenSettingUpdated(object newValue)
        {
            int index = (newValue is int value ? value : 0); ;
            //index might be, for instance, -1 if we're relying on a default value. In this case, don't try to set it.
            if (index >= 0 && index < GetAvailableResolutions().Count)
            {
                var resolutionToSet = GetAvailableResolutions()[index];
                SetResolution(resolutionToSet);
            }
        }

        protected void SetResolution(Resolution resolution)
        {
               Screen.SetResolution(resolution.width,resolution.height, true);
               Registry.Get<Config>().PersistConfigValue("ResolutionDescription",resolution.width + "x" + resolution.height);
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
            return Screen.currentResolution.height;
        }
        /// <summary>
        /// we use Screen.width rather than CurrentResolution.width because Screen.height respects the resolution set in the Editor
        /// </summary>
        /// <returns></returns>
        public int GetScreenWidth()
        {
            return Screen.currentResolution.width;
        }



    }
}