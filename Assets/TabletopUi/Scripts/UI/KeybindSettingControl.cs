using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.TabletopUi.Scripts.UI;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeybindSettingControl : AbstractSettingControl
{
    [SerializeField] public TextMeshProUGUI ActionLabel;
    [SerializeField] public TMP_InputField KeybindingValue;

    [SerializeField]
    public InputActionAsset inputActionAsset;

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
        ActionLabel.text = strategy.SettingHint;
        var action= inputActionAsset.FindAction(strategy.SettingId);
        KeybindingValue.text = action.GetBindingDisplayString();
        _initialisationComplete = true;

    }

    public void OnInputSelect()
    {
         inputActionAsset.FindActionMap("Default").Disable();
        var action = inputActionAsset.FindAction(strategy.SettingId);
        var rebinding = action.PerformInteractiveRebinding().WithControlsExcluding("mouse");
        rebinding.OnComplete(r =>
        {
            KeybindingValue.text = r.selectedControl.displayName;
            
               inputActionAsset.FindActionMap("Default").Enable();
            r.Dispose();
        });


        rebinding.Start();
    }

    public void OnInputDeselect()
    {

    }

    public  void OnValueChanged(string changingToValue)
    {


    }


    public override void Update()
    {

    }



}
