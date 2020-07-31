using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using TMPro;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure.Modding
{
    public class ModEntry : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        public TextMeshProUGUI activationToggleText;
        public Babelfish activationToggleBabel;
        
        private Mod _mod;

        public void ToggleActivation()
        {
            var modManager = Registry.Retrieve<ModManager>();
            _mod.Enabled = !_mod.Enabled;
            modManager.SetModEnableState(_mod.Id, _mod.Enabled);
            UpdateEnablementDisplay();
            
        }
        
        public void Initialize(Mod mod)
        {
            _mod = mod;
            title.text = mod.Name + " (" + mod.Version + ")";
            description.text = mod.Description;
            UpdateEnablementDisplay();
        }

        private void UpdateEnablementDisplay()
        {
            var newLabel = _mod.Enabled ? "UI_DISABLE" : "UI_ENABLE";
            activationToggleBabel.SetLocLabel(_mod.Enabled ? "UI_DISABLE" : "UI_ENABLE");
            activationToggleText.text = LanguageTable.Get(newLabel);
            var newColor = _mod.Enabled ? Color.white : Color.gray;
            title.color = newColor;
            description.color = newColor;
        }

        public void UploadModToStorefront()
        {
            var storefrontServicesProvider=Registry.Retrieve<StorefrontServicesProvider>();
       storefrontServicesProvider.UploadModForCurrentStorefront(_mod);

        }

    }
}