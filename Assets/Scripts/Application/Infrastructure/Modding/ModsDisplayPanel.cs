using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants.Modding;
using SecretHistories.Enums;
using SecretHistories.UI;
using TMPro;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Infrastructure.Modding
{
    public class ModsDisplayPanel: MonoBehaviour
    {
        
        [SerializeField]
        private GameObject modEntryPrefab;
        
        [SerializeField]
        private Transform modEntries;

        [SerializeField]
        public TextMeshProUGUI restartAfterModChangeWarning;

        [SerializeField]
        private TextMeshProUGUI modEmptyMessage;

        private Storefront _store;


        public void Initialise(Storefront store)
        {
            _store = store;
            RebuildModsDisplay();


            //display 'no mods' if no mods are available to enable
            modEmptyMessage.enabled = !Watchman.Get<ModManager>().GetCataloguedMods().Any();
        }


        public void RebuildModsDisplay()
        {
            foreach (Transform modEntry in modEntries)
                Destroy(modEntry.gameObject);

            var modManager = Watchman.Get<ModManager>();
            var modsInOrder = modManager.GetEnabledModsInLoadOrder().ToList();
            var disabledMods = modManager.GetDisabledMods();
            modsInOrder.AddRange(disabledMods); //they need to go after the enabled mods, though we don't care about the order after that

            foreach (var mod in modsInOrder)
            {
                var modEntry = Instantiate(modEntryPrefab).GetComponent<ModEntry>();
                modEntry.name = $"{mod.Id}";
                modEntry.ModEnabled.AddListener(ModWasEnabled);
                modEntry.ModDisabled.AddListener(ModWasDisabled);
                modEntry.ModOrderChanged.AddListener(ModOrderChanged);
                modEntry.transform.SetParent(modEntries, false);
                modEntry.Initialise(mod, _store);
            }

        }

        private void ModWasEnabled()
        {
            TriggerRestartWarning();
            RebuildModsDisplay();
        }

        private void ModWasDisabled()
        {

            TriggerRestartWarning();
            RebuildModsDisplay();
        }

        private void ModOrderChanged()
        {

            RebuildModsDisplay();
        }

        private void TriggerRestartWarning()
        {
            restartAfterModChangeWarning.gameObject.SetActive(true);
    
        }
    }
}
