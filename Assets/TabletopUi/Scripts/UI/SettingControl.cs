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
        private float? newSettingValueQueued = null;

        public string TabId
        {
            get { return strategy.SettingTabId; }
        }

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

        public void OnValueChanged(float changingToValue)
        {
            //I added this guard clause because otherwise the OnValueChanged event can fire while the slider initial values are being set -
            //for example, if the minvalue is set to > the default control value of 0. This could be fixed by
            //adding the listener in code rather than the inspector, but I'm hewing away from that. It could also be 'fixed' by changing the
            //order of the initialisation steps, but that's half an hour of my time I don't want to lose again next time I fiddle :) - AK
            if(_initialisationComplete)
            {
                SoundManager.PlaySfx("UISliderMove");
                newSettingValueQueued = changingToValue;
                 string newValueLabel=strategy.GetLabelForValue((float)newSettingValueQueued);
                 SliderValueLabel.text = newValueLabel;
            }
        }



        public void Update()
        {
            //eg: we don't want to change  resolution until the mouse button is released
            if (!Input.GetMouseButton(0) && newSettingValueQueued!=null)
            {
                strategy.OnSliderValueChangeComplete((float) newSettingValueQueued);
                newSettingValueQueued = null;
            }
        }

        
        }

    }

