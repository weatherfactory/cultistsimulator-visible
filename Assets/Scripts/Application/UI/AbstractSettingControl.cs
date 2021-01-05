using SecretHistories.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.UI
{
    public abstract class AbstractSettingControl : MonoBehaviour
    {
        protected bool _initialisationComplete = false;
        protected float? newSettingValueQueued = null;
        public abstract string SettingId { get; }
        public abstract string TabId { get; }
        public abstract void Initialise(Setting settingToBind);
        public abstract void Update();


        public abstract void SetInteractable(bool interactable);
    }
}