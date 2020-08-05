using TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TabletopUi.Scripts.UI

{
    public class MenuLegacyStartEntry : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;

        public Image installedImage;
        public Image notInstalledImage;
        public BabelfishTemplate storeLink;
        

        //private const string DlcTitleLocLabelPrefix = "UI_DLC_TITLE_";
        private const string DlcDescriptionLocLabelPrefix = "UI_DLC_DESCRIPTION_";
        private const string PurchaseLocLabel = "UI_DLC_PURCHASE";
        private const string ComingSoonLocLabel = "UI_DLC_COMING_SOON";
        private NewStartLegacySpec _spec;
        private string _storeLinkUrl;
        private MenuScreenController _menuScreenController;
        public void Initialize(NewStartLegacySpec spec, Storefront store, bool isInstalled,MenuScreenController menuScreenController)
        {
            _spec = spec;
            _menuScreenController = menuScreenController;
            name = "NewStartLegacy_" + spec.Id;

            title.text = spec.Legacy.Label;
         //   description.text = spec.Legacy.Description; //could use for a short description

            installedImage.sprite = ResourcesManager.GetSpriteForLegacy(spec.Legacy.Image);

           // installedImage.sprite = ResourcesManager.GetSpriteForDlc(spec.Id, true);
            //notInstalledImage.sprite = ResourcesManager.GetSpriteForDlc(spec.Id, false);

            installedImage.gameObject.SetActive(isInstalled);
            notInstalledImage.gameObject.SetActive(!isInstalled);

            description.gameObject.SetActive(isInstalled);
            storeLink.gameObject.SetActive(!isInstalled);

          //  installedLabel.SetTemplate($"<i>{{{DlcDescriptionLocLabelPrefix}{spec.Id.ToUpper()}}}\n</i>");
            if (spec.Links.TryGetValue(store, out _storeLinkUrl))
            {
                if(spec.Released)
                    storeLink.SetTemplate($"<link=\"{_storeLinkUrl}\"><b><u>{{{PurchaseLocLabel}}}</u></b></link>");
                else
                    storeLink.SetTemplate($"<link=\"{_storeLinkUrl}\"><b><u>{{{ComingSoonLocLabel}}}</u></b></link>");
            }
            else
            {
                storeLink.gameObject.SetActive(false);
            }
        }



        public bool IsInstalled()
        {
            return installedImage.isActiveAndEnabled;
        }

        public void TryBeginLegacy()
        {
            if(IsInstalled())
            {
                _menuScreenController.ShowStartDLCLegacyConfirmPanel(_spec.Legacy);
            }

        }

        public void OpenStorepage()
        {
            SoundManager.PlaySfx("UIButtonClick");
            Application.OpenURL(_storeLinkUrl);
        }
    }
}
