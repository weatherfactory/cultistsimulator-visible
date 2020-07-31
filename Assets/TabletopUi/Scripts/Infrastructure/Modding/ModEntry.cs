using System;
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
        public Babelfish uploadToggleBabel;

        public Button activationToggleButton;
        public TextMeshProUGUI activationToggleText;
        public Babelfish activationToggleBabel;
        

        private Mod _mod;


        
        public void Initialize(Mod mod)
        {
            _mod = mod;
            title.text = mod.Name + " (" + mod.Version + ")";
            description.text = mod.Description;
            UpdateEnablementDisplay();


            uploadButton.onClick.AddListener(UploadModToStorefront);
            activationToggleButton.onClick.AddListener(ToggleActivation);
            

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
            _mod.Enabled = !_mod.Enabled;
            modManager.SetModEnableState(_mod.Id, _mod.Enabled);
            UpdateEnablementDisplay();

        }


        public void ModUploaded(ModUploadedArgs args)
        {
            uploadText.text = args.PublishedFileId;
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