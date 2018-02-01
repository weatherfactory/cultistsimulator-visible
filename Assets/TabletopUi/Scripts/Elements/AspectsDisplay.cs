#pragma warning disable 0649
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
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

        [SerializeField] private TextMeshProUGUI Header;

        public void ShowHeader(bool show) {
            if (Header != null) //not all aspects displays have headers
                Header.enabled = show;
        }

        public void DisplayAspects(IAspectsDictionary aspects) {
            ClearCurrentlyDisplayedAspects();

            ShowHeader(aspects.Keys.Any());

            foreach (string k in aspects.Keys)
                AddAspectToDisplay(k, aspects[k]);
        }

        private void AddAspectToDisplay(string aspectId, int quantity) {
            AspectFrame newAspectFrame = PrefabFactory.CreateLocally<AspectFrame>(transform);
            Element aspect = Registry.Retrieve<ICompendium>().GetElementById(aspectId);
            newAspectFrame.PopulateDisplay(aspect, quantity);
        }

        public void ClearCurrentlyDisplayedAspects() {
            foreach (AspectFrame a in GetComponentsInChildren<AspectFrame>())
                DestroyObject(a.gameObject);
        }
    
    }




}
