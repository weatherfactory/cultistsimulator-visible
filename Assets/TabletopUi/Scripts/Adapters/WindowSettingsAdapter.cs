using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
    public class WindowSettingsAdapter: MonoBehaviour,ISettingSubscriber
    {
        public void Initialise()
        {
            var windowedStateSetting = Registry.Get<ICompendium>().GetEntityById<Setting>(NoonConstants.WINDOWED);
            if (windowedStateSetting != null)
            {
                windowedStateSetting.AddSubscriber(this);
                UpdateValueFromSetting(windowedStateSetting.CurrentValue);
            }
            else
                NoonUtility.Log("Missing setting entity: " + NoonConstants.WINDOWED);

        }


        public void UpdateValueFromSetting(object newValue)
        {
            if(newValue is float)
                SetWindowed((float)newValue > 0.5f);
            else
              SetWindowed(false);

        }


        protected void SetWindowed(bool windowed)
        {
            if (windowed)
                Screen.SetResolution(Screen.width, Screen.height, false);
            else
                Screen.SetResolution(Screen.width, Screen.height, true);

        }
    }
}