using System.Collections.Generic;
using System.Linq;
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
            Element aspect = Registry.compendium.GetElementById(aspectId);
            newAspectFrame.PopulateDisplay(aspect, quantity);

        }

        public void ResetAspects()
        {
            foreach (AspectFrame a in GetComponentsInChildren<AspectFrame>())
            DestroyObject(a.gameObject);

        }

        public void DisplayAspects(Dictionary<string,int> aspects)
        {

            ResetAspects();
                foreach (string k in aspects.Keys)
                {
                AddAspectToDisplay(k,aspects[k]);
                }
            }
    
    }
}
