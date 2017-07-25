using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.TabletopUi.Scripts
{
    /// <summary>
    /// displays a summary of aspects; used for the workspace display, and in the recipe book
    /// </summary>
    public class AspectsDisplay : MonoBehaviour
    {

        private void AddAspectToDisplay(string aspectId, int quantity)
        {
            AspectFrame newAspectFrame = PrefabFactory.CreateLocally<AspectFrame>(transform);
            Element aspect = Registry.Retrieve<ICompendium>().GetElementById(aspectId);
            newAspectFrame.PopulateDisplay(aspect, quantity);

        }

        public void ClearAspects()
        {
            foreach (AspectFrame a in GetComponentsInChildren<AspectFrame>())
            DestroyObject(a.gameObject);

        }

        public void DisplayAspects(IAspectsDictionary aspects) {
            ClearAspects();

            foreach (string k in aspects.Keys) 
                AddAspectToDisplay(k,aspects[k]);

            gameObject.SetActive(aspects.Count > 0);
        }
    
    }
}
