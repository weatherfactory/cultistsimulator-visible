using Assets.CS.TabletopUI;
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
        public TextMeshProUGUI storeLink;
        

        private const string PurchaseLocLabel = "UI_DLC_PURCHASE";
        private const string ComingSoonLocLabel = "UI_DLC_COMING_SOON";
        private StartableLegacySpec _spec;
        private string _storeLinkUrl;
        private MenuScreenController _menuScreenController;
        public void Initialize(StartableLegacySpec spec, Storefront store, MenuScreenController menuScreenController)
        {
            _spec = spec;
            _menuScreenController = menuScreenController;
            name = "NewStartLegacy_" + spec.Id;

           
            if (spec.Legacy != null)
            {
                title.text = spec.Legacy.Label;
                installedImage.sprite = ResourcesManager.GetSpriteForLegacy(spec.Legacy.Image);
                installedImage.gameObject.SetActive(true);
                notInstalledImage.gameObject.SetActive(false);
                storeLink.gameObject.SetActive(false);
            }
            else
            {
                title.text = Registry.Get<ILocStringProvider>().Get(spec.LocLabelIfNotInstalled);
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


            if (spec.ReleasedByWf && !IsInstalled() && spec.Links.TryGetValue(store, out _storeLinkUrl))
            {
                storeLink.gameObject.SetActive(true);
                string linkText = Registry.Get<LanguageManager>().Get(PurchaseLocLabel);
                storeLink.text = $"<link=\"{_storeLinkUrl}\"><b><u>{linkText}</u></b></link>";

            }
            else
                storeLink.gameObject.SetActive(false);
            
        }



        public bool IsInstalled()
        {
            return _spec.Legacy != null;
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
