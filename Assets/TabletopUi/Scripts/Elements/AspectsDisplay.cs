#pragma warning disable 0649
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;

namespace Assets.TabletopUi.Scripts
{
    /// <summary>
    /// displays a summary of aspects; used for the workspace display, and in the recipe book
    /// </summary>
    public class AspectsDisplay : MonoBehaviour {

        // This is pushed to the Aspect Frame 
        // There it is used in the click>Notifier call to tell the notifier where to place the details window
        [SerializeField] bool isWithinDetailsWindow;
        [SerializeField] bool hideIfEmpty;
        [SerializeField] bool hasBrightBackground;
        [SerializeField] private TextMeshProUGUI Header;

        public void ShowHeader(bool show) {
            if (Header != null) //not all aspects displays have headers
                Header.enabled = show;
        }

        public virtual void DisplayAspects(IAspectsDictionary aspects) {            
            ClearCurrentlyDisplayedAspects();

            bool anyAspects = aspects != null && aspects.Keys.Any();
                        
            gameObject.SetActive(hideIfEmpty ? anyAspects : true);
            ShowHeader(anyAspects);

            if (anyAspects)
                foreach (string k in aspects.Keys)
                    AddAspectToDisplay(k, aspects[k]);
        }

        private void AddAspectToDisplay(string aspectId, int quantity) {
            Element aspect = Registry.Retrieve<ICompendium>().GetEntityById<Element>(aspectId);

            if (aspect == null)
                return; // We can't find the aspect? Well then don't add anything

            if (aspect.IsHidden)
                return; //...because it's hidden

            ElementFrame newElementFrame = PrefabFactory.CreateLocally<ElementFrame>(transform);
            newElementFrame.PopulateDisplay(aspect, quantity, null,hasBrightBackground);

            if (isWithinDetailsWindow)
                newElementFrame.SetAsDetailWindowChild();
        }

        public virtual void ClearCurrentlyDisplayedAspects() {
            foreach (ElementFrame a in GetComponentsInChildren<ElementFrame>())
                Object.Destroy(a.gameObject);
        }

    }




}
