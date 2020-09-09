using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine.InputSystem;

namespace Assets.TabletopUi.Scripts.UI
{
    public class KeybindSettingControlStrategy : SettingControlStrategy
    {
        public void Rebind(InputActionAsset inputActionAsset,
            TMP_InputField input)
        {
            inputActionAsset.actionMaps[0].Disable();
            var action = inputActionAsset.FindAction(SettingId);
            var rebinding = action.PerformInteractiveRebinding().WithControlsExcluding("mouse");
            rebinding.OnComplete(r =>
            {
                input.text = r.selectedControl.displayName;
                input.DeactivateInputField();
                inputActionAsset.actionMaps[0].Enable();
                ChangeSettingArgs args = new ChangeSettingArgs
                {
                    Key = boundSetting.Id,
                    Value = action.bindings[0].overridePath
                };

                Registry.Get<Concursum>().ChangeSetting(args);


                r.Dispose();
            });


            rebinding.Start();

        }


    }
}