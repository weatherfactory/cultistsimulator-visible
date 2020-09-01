using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
    public class WindowSettingsAdapter: MonoBehaviour,ISettingSubscriber
    {
        public void UpdateValueFromSetting(float newValue)
        {
            SetWindowed(newValue>0.5f);
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