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
                ChangeSettingArgs changeSettingArgs = new ChangeSettingArgs
                {
                    Key = boundSetting.Id,
                    Value = action.bindings[0].overridePath
                };

                Registry.Get<Concursum>().ChangeSetting(changeSettingArgs);
                Registry.Get<Concursum>().ContentUpdatedEvent.Invoke(new ContentUpdatedArgs{Message = "Changed a key binding, which might need reflecting in on-screen prompts"});

                r.Dispose();
            });


            rebinding.Start();

        }


    }
}