using System.Collections.Generic;
using TabletopUi.Scripts.Services;
using UnityEngine;
using UnityEngine.UI;

namespace TabletopUi.Scripts.UI
{
    public class MenuDlcEntry : MonoBehaviour
    {
        public Babelfish title;
        public Image icon;
        public BabelfishTemplate storeLink;
        public BabelfishTemplate installedLabel;

        private const string DlcTitleLocLabelPrefix = "UI_DLC_TITLE_";
        private const string DlcDescriptionLocLabelPrefix = "UI_DLC_DESCRIPTION_";
        private const string InstalledLocLabel = "UI_DLC_INSTALLED";
        private const string PurchaseLocLabel = "UI_DLC_PURCHASE";
        private Spec _spec;
        private MenuScreenController _menuScreenController;
        public void Initialize(Spec spec, Storefront store, bool isInstalled,MenuScreenController menuScreenController)
        {
            _spec = spec;
            _menuScreenController = menuScreenController;
            name = "DLCEntry_" + spec.Id;
            title.SetLocLabel(DlcTitleLocLabelPrefix + spec.Id);
            icon.sprite = ResourcesManager.GetSpriteForDlc(spec.Id, isInstalled);

            installedLabel.gameObject.SetActive(isInstalled);
            storeLink.gameObject.SetActive(!isInstalled);

            installedLabel.SetTemplate($"<i>{{{DlcDescriptionLocLabelPrefix}{spec.Id}}}\n<b>{{{InstalledLocLabel}}}</b></i>");
            if (spec.StoreLinks.TryGetValue(store, out var storeLinkUrl))
            {
                storeLink.SetTemplate($"<link=\"{storeLinkUrl}\"><b><u>{{{PurchaseLocLabel}}}</u></b></link>");
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

            public Spec(string id, Dictionary<Storefront, string> storeLinks)
            {
                Id = id;
                StoreLinks = storeLinks;
            }
        }


        public void TryBeginDLC()
        {
            _menuScreenController.ShowStartDLCLegacyConfirmPanel(_spec.Id.ToLower());
            Debug.Log("click worked for " + _spec.Id.ToLower());

        }
    }
}
