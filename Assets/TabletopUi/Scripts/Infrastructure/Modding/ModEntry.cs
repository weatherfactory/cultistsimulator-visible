#if MODS
using Assets.CS.TabletopUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Infrastructure.Modding
{
    public class ModEntry : MonoBehaviour
    {
        public Text activationToggle;
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        
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
            activationToggle.text = _mod.Enabled ? "Disable" : "Enable";
            var newColor = _mod.Enabled ? Color.white : Color.gray;
            title.color = newColor;
            description.color = newColor;
        }
    }
}
#endif