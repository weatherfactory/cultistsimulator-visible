using Assets.CS.TabletopUI;
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
            modManager.SetModEnabled(_mod.Id, !_mod.Enabled);
            UpdateActivationState();
            
            // Update the compendium content
            var registry = new Registry();
            var compendium = new Compendium();
            registry.Register<ICompendium>(compendium);
            var contentImporter = new ContentImporter();
            contentImporter.PopulateCompendium(compendium);
        }
        
        public void Initialize(Mod mod)
        {
            _mod = mod;
            title.text = mod.Name + " (" + mod.Version + ")";
            description.text = mod.Description;
            UpdateActivationState();
        }

        private void UpdateActivationState()
        {
            var newLabel = _mod.Enabled ? "UI_DISABLE" : "UI_ENABLE";
            activationToggleBabel.SetLocLabel(_mod.Enabled ? "UI_DISABLE" : "UI_ENABLE");
            activationToggleText.text = LanguageTable.Get(newLabel);
            var newColor = _mod.Enabled ? Color.white : Color.gray;
            title.color = newColor;
            description.color = newColor;
        }
    }
}