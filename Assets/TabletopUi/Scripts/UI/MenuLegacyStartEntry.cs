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

           bool isInstalled = spec.Legacy != null;

            if (isInstalled)
            {
                title.text = spec.Legacy.Label;
                installedImage.sprite = ResourcesManager.GetSpriteForLegacy(spec.Legacy.Image);
                installedImage.gameObject.SetActive(true);
                notInstalledImage.gameObject.SetActive(false);
                storeLink.gameObject.SetActive(false);
            }
            else
            {
                title.text = LanguageTable.Get(spec.LocLabelIfNotInstalled);
                notInstalledImage.sprite = ResourcesManager.GetSpriteForLegacy("_" + spec.Id);
                installedImage.gameObject.SetActive(false);
                notInstalledImage.gameObject.SetActive(true);
                storeLink.gameObject.SetActive(true);
            }

            if (!spec.ReleasedByWf)
            {
                title.color = new Color32(253, 111, 191, 255);
                title.text += " [MOD]";
            }


            if (spec.ReleasedByWf && spec.Links.TryGetValue(store, out _storeLinkUrl))
                storeLink.SetTemplate($"<link=\"{_storeLinkUrl}\"><b><u>{{{PurchaseLocLabel}}}</u></b></link>");
            else
                storeLink.gameObject.SetActive(false);
            
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
