using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.UI;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeybindSettingControl : AbstractSettingControl
{
    [SerializeField] public TextMeshProUGUI ActionLabel;
    [SerializeField] public TMP_InputField keybindingInputField;

    [SerializeField]
    public InputActionAsset inputActionAsset;

    private KeybindSettingControlStrategy strategy;

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
        ActionLabel.text = Registry.Get<LanguageManager>().Get(strategy.SettingHint);
        var action= inputActionAsset.FindAction(strategy.SettingId);
        keybindingInputField.text = action.controls[0].displayName;
        _initialisationComplete = true;
    }

    public void OnInputSelect()
    {
        keybindingInputField.text=String.Empty;
        strategy.Rebind(inputActionAsset,keybindingInputField);
    }





    public override void Update()
    {

    }



}
