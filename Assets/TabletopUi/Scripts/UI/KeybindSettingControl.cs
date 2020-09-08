using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.TabletopUi.Scripts.UI;
using Noon;
using TMPro;
using UnityEngine;

public class KeybindSettingControl : AbstractSettingControl
{
    [SerializeField] public TextMeshProUGUI ActionLabel;
    [SerializeField] public TextMeshProUGUI KeybindValueLabel;

    private SettingControlStrategy strategy;

    public override string TabId
    {
        get { return strategy.SettingTabId; }
    }

    // Start is called before the first frame update
    public override void Initialise(Setting settingToBind)
    {
        if (settingToBind == null)
        {
            NoonUtility.Log("Trying to create a keybind setting control with a null setting entity");
            return;
        }

        strategy = new KeybindSettingControlStrategy();
        strategy.Initialise(settingToBind);
        gameObject.name = "KeybindSetting_" + strategy.SettingId;
        ActionLabel.name = strategy.SettingHint;
        _initialisationComplete = true;

    }

    public override void OnValueChanged(float changingToValue)
    {

        //if (_initialisationComplete)
        //{
        //    SoundManager.PlaySfx("UISliderMove");
        //    newSettingValueQueued = changingToValue;
        //    string newValueLabel = strategy.GetLabelForValue((float)newSettingValueQueued);
        //    SliderValueLabel.text = newValueLabel;
        //}
    }

    public override void Update()
    {

    }



}
