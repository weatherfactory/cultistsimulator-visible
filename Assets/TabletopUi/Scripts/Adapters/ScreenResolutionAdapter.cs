using System;
using System.Collections.Generic;
using Assets.Core.Entities;
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

        public void Start()
        {
            var registry = new Registry();
            registry.Register(this);


            var resolutionSetting = Registry.Get<ICompendium>().GetEntityById<Setting>(NoonConstants.RESOLUTION);
            if (resolutionSetting != null)
            {
                resolutionSetting.AddSubscriber(this);
                UpdateValueFromSetting(resolutionSetting.CurrentValue);
            }
            else
                NoonUtility.Log("Missing setting entity: " + NoonConstants.RESOLUTION);


        }

        public void UpdateValueFromSetting(float newValue)
        {
            SetResolution(GetAvailableResolutions()[(int)newValue]);
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