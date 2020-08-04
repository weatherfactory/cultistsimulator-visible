using System;
using System.Linq;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Infrastructure.Modding
{

    public class ModUploadedArgs
    {
        public string PublishedFileId { get; set; }
    }
    public class ModEntry : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;

        public Button uploadButton;
        public TextMeshProUGUI uploadText;
        public Babelfish uploadBabel;

        public Button activationToggleButton;
        public TextMeshProUGUI activationToggleText;
        public Babelfish activationToggleBabel;

        public Image SteamImage;
        public Image LocalImage;
        

        private Mod _mod;


        
        public void Initialize(Mod mod)
        {
            _mod = mod;

            title.text = _mod.Name + " (" + mod.Version + ")";
            description.text = _mod.Description;
            UpdateEnablementDisplay();

            uploadButton.onClick.AddListener(UploadModToStorefront);
            activationToggleButton.onClick.AddListener(ToggleActivation);


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
            activationToggleBabel.SetLocLabel(_mod.Enabled ? "UI_DISABLE" : "UI_ENABLE");
            activationToggleText.text = LanguageTable.Get(newLabel);
            var newColor = _mod.Enabled ? Color.white : Color.gray;
            title.color = newColor;
            description.color = newColor;
        }

        public void ToggleActivation()
        {
            var modManager = Registry.Retrieve<ModManager>();

            //can't enable two mods with the same name - this would usually be both a local and a Steam version
            if (!_mod.Enabled && modManager.GetEnabledMods().ToList().Exists(m => m.Name == _mod.Name))
                NoonUtility.Log($"Can't enable two mods with the same name ({_mod.Name})", 1);
            else
            {
                _mod.Enabled = !_mod.Enabled;
                modManager.SetModEnableState(_mod.Id, _mod.Enabled);
                UpdateEnablementDisplay();
            }
        }

        public void SetUploadButtonState()
        {
            if (_mod.ModInstallType != ModInstallType.Local)
            {
                uploadButton.gameObject.SetActive(false);
                return;
            }

            var modManager = Registry.Retrieve<ModManager>();
            string publishedFileId=modManager.GetPublishedFileIdForMod(_mod);
            if (string.IsNullOrEmpty(publishedFileId))
            {
                uploadButton.gameObject.SetActive(true);
                uploadBabel.SetLocLabel("UI_UPLOAD");
                uploadText.text = LanguageTable.Get("UI_UPLOAD");
            }
            else
            {
                uploadButton.gameObject.SetActive(true);
                uploadBabel.SetLocLabel("UI_UPDATE");
                uploadText.text = LanguageTable.Get("UI_UPDATE");
                
            }

        }


        public void ModUploaded(ModUploadedArgs args)
        {
            var modManager = Registry.Retrieve<ModManager>();
            modManager.TryWritePublishedFileId(_mod, args.PublishedFileId);
            SetUploadButtonState();
        }

        public async void UploadModToStorefront()
        {
          //  AsyncCallback callBack=new AsyncCallback(ModUploadComplete);
            var storefrontServicesProvider=Registry.Retrieve<StorefrontServicesProvider>();
            Action<ModUploadedArgs> modUploadedAction = ModUploaded;
            uploadText.text = "sec...";
            await storefrontServicesProvider.UploadModForCurrentStorefront(_mod,modUploadedAction);
        }


    }
}