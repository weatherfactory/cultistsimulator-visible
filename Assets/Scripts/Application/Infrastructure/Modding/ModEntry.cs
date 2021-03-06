using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Infrastructure.Modding;
using SecretHistories.UI;
using SecretHistories.Services;

using SecretHistories.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SecretHistories.Constants.Modding
{


    public class ModEntry : MonoBehaviour
    {
        public UnityEvent ModEnabled;
        public UnityEvent ModDisabled;
        public UnityEvent ModOrderChanged;
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;

        public Button uploadButton;
        public TextMeshProUGUI uploadText;
        public Babelfish uploadBabel;

        public Button activationToggleButton;
        public TextMeshProUGUI activationToggleText;
        public Babelfish activationToggleBabel;


        public Button higherPriorityButton;
        public Button lowerPriorityButton;

        public Image SteamImage;
        public Image LocalImage;
        public Image PreviewImage;

        public string ModId => _mod?.Id ?? string.Empty;

        public bool EnabledMod => _mod?.Enabled ?? false; //not to be confused with the monobehaviour property 'enabled'

        private Mod _mod;
        private Storefront _store;


        
        public void Initialise(Mod mod,Storefront store)
        {

            _mod = mod;
            _store = store;

            title.text = _mod.Name + " (" + mod.Version + ")";
            description.text = _mod.Description;
            if (_mod.PreviewImage != null)
                PreviewImage.overrideSprite = _mod.PreviewImage;

            UpdateEnablementDisplay();
            

            var concursum = Watchman.Get<Concursum>();

            concursum.ModOperationEvent.AddListener(ModOperationEvent);


            if (mod.ModInstallType == ModInstallType.Local)
            {
                LocalImage.gameObject.SetActive(true);
                SteamImage.gameObject.SetActive(false);
                SetUploadButtonState();
            }
            else if(mod.ModInstallType==ModInstallType.SteamWorkshop)
            {
                LocalImage.gameObject.SetActive(false);
                SteamImage.gameObject.SetActive(true);
                uploadButton.gameObject.SetActive(false);
                SetUploadButtonState();
            }
            else
            {
                LocalImage.gameObject.SetActive(false);
                SteamImage.gameObject.SetActive(false);
                SetUploadButtonState();
                NoonUtility.Log($"Problematic install type for mod {_mod.Id} {_mod.Name} - {mod.ModInstallType}",1);
            }
             
        }



        private void UpdateEnablementDisplay()
        {
            var newLabel = _mod.Enabled ? "UI_DISABLE" : "UI_ENABLE";
            activationToggleBabel.UpdateLocLabel(_mod.Enabled ? "UI_DISABLE" : "UI_ENABLE");
            activationToggleText.text = Watchman.Get<ILocStringProvider>().Get(newLabel);
            var newTextColor = _mod.Enabled ? Color.white : Color.gray;
            title.color = newTextColor;
            description.color = newTextColor;

            higherPriorityButton.gameObject.SetActive(_mod.Enabled);
            lowerPriorityButton.gameObject.SetActive(_mod.Enabled);


        }

        public void ToggleActivation()
        {

            var modManager = Watchman.Get<ModManager>();

            //can't enable two mods with the same name - this would usually be both a local and a Steam version
            if (!_mod.Enabled && modManager.GetEnabledModsInLoadOrder().ToList().Exists(m => m.Name == _mod.Name))
                NoonUtility.Log($"Can't enable two mods with the same name ({_mod.Name})", 1);
            else
            {
                _mod=modManager.SetModEnableStateAndReloadContent(_mod.Id, !_mod.Enabled); //the mod should come back from here, reloaded and refreshed, with its enabled/disabled flag set appropriately.
                UpdateEnablementDisplay();
                ModEnabled.Invoke();
            }

            //look for the first disabled mod in entries that is not this one.
            var modEntries = gameObject.transform.parent.GetComponentsInChildren<ModEntry>();
            var firstDisabledMod = modEntries.FirstOrDefault(m => !m.EnabledMod && m.ModId != _mod.Id);
            if (firstDisabledMod != null)
            {
                //move to the position immediately above the first disabled mod (whether we were just enabled or just disabled)
                var firstDisabledModSiblingIndex = firstDisabledMod.gameObject.transform.GetSiblingIndex();
        if(firstDisabledModSiblingIndex==0)
            gameObject.transform.SetAsFirstSibling();
        else
               gameObject.transform.SetSiblingIndex(firstDisabledModSiblingIndex-1);
            }
            else
            {
                //move to the last position. If the mod has just been enabled, it was the only disabled entry, so it's already at the end. If it has just been disabled, it should go to the end.
                gameObject.transform.SetAsLastSibling();
            }

            //A lot less simple than it looked. If it doesn't work as intended, or stops working as intended, I should have a modpanel class instead. See TriggerRestartWarning above.
        }

        public void IncreaseModPriority()
        {
            var modManager = Watchman.Get<ModManager>();
            var enabledModsInOriginalOrder = new List<Mod>(modManager.GetEnabledModsInLoadOrder());

            var thisModIndex = enabledModsInOriginalOrder.IndexOf(_mod);

            if (thisModIndex >0)
            {
                int swapWithModIndex = thisModIndex - 1;
                modManager.SwapModsInLoadOrderAndPersistToFile(thisModIndex, swapWithModIndex);
            }
            ModOrderChanged.Invoke();

        }

        public void DecreaseModPriority()
        {
            var modManager = Watchman.Get<ModManager>();
            var enabledModsInOriginalOrder = new List<Mod>(modManager.GetEnabledModsInLoadOrder());

            var thisModIndex = enabledModsInOriginalOrder.IndexOf(_mod);

            if (thisModIndex+1 < enabledModsInOriginalOrder.Count) //if the zero-based index were one higher, would it still be less than the one-based count?
            {
                int swapWithModIndex = thisModIndex + 1;

                modManager.SwapModsInLoadOrderAndPersistToFile(thisModIndex, swapWithModIndex);
                
               ModOrderChanged.Invoke();
            }

        }


        public void SetUploadButtonState()
        {
            var storefrontServicesProvider = Watchman.Get<StorefrontServicesProvider>();
            uploadButton.interactable = true;

            if (_mod.ModInstallType != ModInstallType.Local // can't upload unless it's installed locally
                || _store != Storefront.Steam //at time of coding, only Steam supports uploads
        ||   !storefrontServicesProvider.IsAvailable(StoreClient.Steam // and we need to be able to reach Steam
                ) )
            {
                uploadButton.gameObject.SetActive(false);
                return;
            }

            string publishedFileId = GetPublishedFileIdForThisMod();

            if (string.IsNullOrEmpty(publishedFileId))
            {
                uploadButton.gameObject.SetActive(true);
                uploadBabel.UpdateLocLabel("UI_UPLOAD");
                uploadText.text = Watchman.Get<ILocStringProvider>().Get("UI_UPLOAD");
            }
            else
            {
                uploadButton.gameObject.SetActive(true);
                uploadBabel.UpdateLocLabel("UI_UPDATE");
                uploadText.text = Watchman.Get<ILocStringProvider>().Get("UI_UPDATE");
                
            }

        }


        public async void UploadModToStorefront()
        {

            var concursum = Watchman.Get<Concursum>();

            if (!File.Exists(_mod.PreviewImageFilePath))
            {
                concursum.ShowNotification(new NotificationArgs{Title="Missing preview image",Description = "The mod needs a 100x100 PNG file named 'cover.png'"});
                return;
            }

            var storefrontServicesProvider = Watchman.Get<StorefrontServicesProvider>();

            uploadText.text = "...";

            concursum.ShowNotification(new NotificationArgs { Title = "Uploading...", Description = "'I will meditate on the triumph of patience over strength.' Please wait a few moments for your files to upload." });
            uploadButton.interactable = false;

            var publishedFileId = Watchman.Get<ModManager>().GetPublishedFileIdForMod(_mod);

            

            if(string.IsNullOrEmpty(GetPublishedFileIdForThisMod()))
                await storefrontServicesProvider.UploadModForCurrentStorefront(_mod);
            else
                await storefrontServicesProvider.UpdateModForCurrentStorefront(_mod,publishedFileId);
        }

        private string GetPublishedFileIdForThisMod()
        {

            var modManager = Watchman.Get<ModManager>();
            string publishedFileId = modManager.GetPublishedFileIdForMod(_mod);
            return publishedFileId;
        }

        public void ModOperationEvent(ModOperationArgs modOperationArgs)
        {
            if (modOperationArgs.Mod.Name != _mod.Name)
                return;

            var modManager = Watchman.Get<ModManager>();

            

            if(modOperationArgs.Successful)
            {
                modManager.TryWritePublishedFileId(_mod, modOperationArgs.PublishedFileId);
                NoonUtility.Log(modOperationArgs.Message);

            }
            else
                NoonUtility.Log(modOperationArgs.Message,1);

            var notificationArgs=new NotificationArgs();
            notificationArgs.Title = "Serapeum Response";
            notificationArgs.Description = modOperationArgs.Message;

            var concursum = Watchman.Get<Concursum>();
            concursum.ShowNotification(notificationArgs);


            SetUploadButtonState();
        }




    }
}