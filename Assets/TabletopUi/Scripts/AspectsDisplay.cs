using System.Collections.Generic;
using System.Linq;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts
{
    /// <summary>
    /// displays a summary of aspects; used for the workspace display, and in the recipe book
    /// </summary>
    public class AspectsDisplay : MonoBehaviour
    {

        [SerializeField]private AspectFrame prefabAspectFrame;


        private void AddAspectToDisplay(string aspectId, int quantity)
        {
            AspectFrame newAspectFrame = gameObject.InstantiateLocally<AspectFrame>(prefabAspectFrame, transform);
            Element aspect = CompendiumHolder.compendium.GetElementById(aspectId);
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

                foreach (KeyValuePair<string, int> kvp in aspects)
                {
                AddAspectToDisplay(kvp.Key, kvp.Value);
                }

            }
        
        private void DisplayRecipesForCurrentAspects()
        {
            //var recipe= CompendiumHolder.compendium.GetFirstRecipeForAspectsWithVerb(AllCurrentAspects(),VERVB);

        }


    }
}
