#pragma warning disable 0649
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.Services;
using TMPro;
using UnityEngine;

namespace SecretHistories.UI
{
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
            Element aspectElement = Watchman.Get<Compendium>().GetEntityById<Element>(aspectId);

            if (aspectElement == null)
                return; // We can't find the aspect? Well then don't add anything

            if (aspectElement.IsHidden)
                return; //...because it's hidden

            ElementFrame newElementFrame = Watchman.Get<PrefabFactory>().CreateLocally<ElementFrame>(transform);
            newElementFrame.PopulateDisplay(aspectElement.Id, quantity,hasBrightBackground);

            if (isWithinDetailsWindow)
                newElementFrame.SetAsDetailWindowChild();
        }

        public virtual void ClearCurrentlyDisplayedAspects() {
            foreach (ElementFrame a in GetComponentsInChildren<ElementFrame>())
                Object.Destroy(a.gameObject);
        }

    }




}
