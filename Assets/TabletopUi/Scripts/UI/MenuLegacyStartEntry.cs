using TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TabletopUi.Scripts.UI

{
    public class MenuLegacyStartEntry : MonoBehaviour
    {
        public TextMeshProUGUI title;

        public Image installedImage;
        public Image notInstalledImage;
        public BabelfishTemplate storeLink;
        

        private const string PurchaseLocLabel = "UI_DLC_PURCHASE";
        private const string ComingSoonLocLabel = "UI_DLC_COMING_SOON";
        private NewStartLegacySpec _spec;
        private string _storeLinkUrl;
        private MenuScreenController _menuScreenController;
        public void Initialize(NewStartLegacySpec spec, Storefront store, MenuScreenController menuScreenController)
        {
            _spec = spec;
            _menuScreenController = menuScreenController;
            name = "NewStartLegacy_" + spec.Id;

            title.text = spec.Legacy.Label;

            bool isInstalled = spec.Legacy != null;

            //   description.text = spec.Legacy.Description; //could use for a short description

            installedImage.sprite = ResourcesManager.GetSpriteForLegacy(spec.Legacy.Image);
            notInstalledImage.sprite = ResourcesManager.GetSpriteForLegacy("_" + spec.Legacy.Image);

            installedImage.gameObject.SetActive(isInstalled);
            notInstalledImage.gameObject.SetActive(!isInstalled);

            storeLink.gameObject.SetActive(!isInstalled);

            if (spec.Links.TryGetValue(store, out _storeLinkUrl))
            {
                if(spec.ReleasedByWf)
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
                _menuScreenController.ShowStartLegacyConfirmPanel(_spec.Legacy);
            }

        }

        public void OpenStorepage()
        {
            SoundManager.PlaySfx("UIButtonClick");
            Application.OpenURL(_storeLinkUrl);
        }
    }
}
