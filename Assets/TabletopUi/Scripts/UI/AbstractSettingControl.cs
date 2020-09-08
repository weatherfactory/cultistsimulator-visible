using Assets.Core.Entities;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.UI
{
    public abstract class AbstractSettingControl : MonoBehaviour
    {


        protected AbstractSettingControlStrategy strategy;
        protected bool _initialisationComplete = false;
        protected float? newSettingValueQueued = null;

        public string TabId
        {
            get { return strategy.SettingTabId; }
        }

        public abstract void Initialise(Setting settingToBind);

        public abstract void OnValueChanged(float changingToValue);



        public abstract void Update();




    }
}