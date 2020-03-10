using System.Collections.Generic;
using TabletopUi.Scripts.Services;
using UnityEngine;
using UnityEngine.UI;

namespace TabletopUi.Scripts.UI
{
    public class MenuDlcEntry : MonoBehaviour
    {
        public Babelfish title;
        public Image installedImage;
        public Image notInstalledImage;
        public BabelfishTemplate storeLink;
        public BabelfishTemplate installedLabel;

        private const string DlcTitleLocLabelPrefix = "UI_DLC_TITLE_";
        private const string DlcDescriptionLocLabelPrefix = "UI_DLC_DESCRIPTION_";
        private const string PurchaseLocLabel = "UI_DLC_PURCHASE";
        private const string ComingSoonLocLabel = "UI_DLC_COMING_SOON";
        private Spec _spec;
        private string _storeLinkUrl;
        private MenuScreenController _menuScreenController;
        public void Initialize(Spec spec, Storefront store, bool isInstalled,MenuScreenController menuScreenController)
        {
            _spec = spec;
            _menuScreenController = menuScreenController;
            name = "DLCEntry_" + spec.Id;
            title.SetLocLabel(DlcTitleLocLabelPrefix + spec.Id);

            installedImage.sprite = ResourcesManager.GetSpriteForDlc(spec.Id, true);
            notInstalledImage.sprite = ResourcesManager.GetSpriteForDlc(spec.Id, false);

            installedImage.gameObject.SetActive(isInstalled);
            notInstalledImage.gameObject.SetActive(!isInstalled);

            installedLabel.gameObject.SetActive(isInstalled);
            storeLink.gameObject.SetActive(!isInstalled);

            installedLabel.SetTemplate($"<i>{{{DlcDescriptionLocLabelPrefix}{spec.Id}}}\n</i>");
            if (spec.StoreLinks.TryGetValue(store, out _storeLinkUrl))
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

        public class Spec
        {
            public string Id { get; }
            public Dictionary<Storefront, string> StoreLinks { get; }
            public bool Released;

            public Spec(string id, Dictionary<Storefront, string> storeLinks,bool released)
            {
                Id = id;
                StoreLinks = storeLinks;
                Released = released;
            }
        }

        public bool IsInstalled()
        {
            return installedImage.isActiveAndEnabled;
        }

        public void TryBeginDLC()
        {
            if(IsInstalled())
            {
            _menuScreenController.ShowStartDLCLegacyConfirmPanel(_spec.Id.ToLower());
            }

        }

        public void OpenStorepage()
        {
            SoundManager.PlaySfx("UIButtonClick");
            Application.OpenURL(_storeLinkUrl);
        }
    }
}
