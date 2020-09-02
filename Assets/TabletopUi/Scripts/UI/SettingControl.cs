using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.UI
{
    public class SettingControl: MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI SliderHint;
        [SerializeField]
        private Slider Slider;
        [SerializeField]
        private TextMeshProUGUI SliderValueLabel;

        private AbstractSettingControlStrategy strategy;
        private bool _initialisationComplete=false;
        private int deferredResolutionChangeToIndex = -1;

        public void Initialise(Setting settingToBind)
        {
            if(settingToBind==null)
            {
                NoonUtility.Log("Trying to create a settingControl with a null setting entity");
                return;
            }


            if(settingToBind.Id==NoonConstants.RESOLUTION)
                strategy=new ResolutionSettingControlStrategy();
            else
                strategy = new FucineSettingControlStrategy();

            strategy.Initialise(settingToBind);
            strategy.SetSliderValues(Slider);
            SliderHint.text = strategy.SettingHint;
            SliderValueLabel.text = strategy.GetLabelForCurrentValue();



            gameObject.name = "SettingControl_" + strategy.SettingId;


            _initialisationComplete = true;

        }

        public void OnValueChanged(float newValue)
        {
            //I added this guard clause because otherwise the OnValueChanged event can fire while the slider initial values are being set -
            //for example, if the minvalue is set to > the default control value of 0. This could be fixed by
            //adding the listener in code rather than the inspector, but I'm hewing away from that. It could also be 'fixed' by changing the
            //order of the initialisation steps, but that's half an hour of my time I don't want to lose again next time I fiddle :) - AK
            if(_initialisationComplete)
            {
                SoundManager.PlaySfx("UISliderMove");
             string newValueLabel=strategy.ChangeValue(newValue);

             SliderValueLabel.text = newValueLabel;

            }
        }


        public void RunAnyDeferredCommands()
        {
            if (deferredResolutionChangeToIndex >= 0)
            {
                var availableResolutions = Registry.Get<ScreenResolutionAdapter>().GetAvailableResolutions();
                //      GraphicsSettingsAdapter.SetResolution(availableResolutions[deferredResolutionChangeToIndex]);
                deferredResolutionChangeToIndex = -1;
            }

        }



        public void Update()
        {
            //eg: we don't want to change  resolution until the mouse button is released
            if (!Input.GetMouseButton(0))
                RunAnyDeferredCommands();
        }

    

        public void SetResolutionDeferred(float value)
        {
            int r = Convert.ToInt32(value);
            PlayerPrefs.SetInt(NoonConstants.RESOLUTION, r);

            if (gameObject.activeInHierarchy == false)
                return; // don't update anything if we're not visible.
            else
            {
                deferredResolutionChangeToIndex = r;
                SoundManager.PlaySfx("UISliderMove");
            }
        }

    }
}
