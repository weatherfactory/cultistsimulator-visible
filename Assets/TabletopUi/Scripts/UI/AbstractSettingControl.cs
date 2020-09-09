using Assets.Core.Entities;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.UI
{
    public abstract class AbstractSettingControl : MonoBehaviour
    {


        protected bool _initialisationComplete = false;
        protected float? newSettingValueQueued = null;

        public abstract string TabId { get; }


        public abstract void Initialise(Setting settingToBind);

//public abstract void OnValueChanged(float changingToValue);



        public abstract void Update();




    }
}