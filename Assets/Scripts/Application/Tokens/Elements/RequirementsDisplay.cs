using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Services;
using SecretHistories.UI;
using TMPro;
using UnityEngine;


namespace SecretHistories.Assets.Scripts.Application.Tokens.Elements
{
   public class RequirementsDisplay: MonoBehaviour
    {

        
        public virtual void DisplayRequirements(Dictionary<string,string>requirements)
        {
            ClearCurrentlyDisplayedRequirements();

            if (!requirements.Any())
            {
                AddAspectToDisplay("null",".");
            }
            else
               foreach (string k in requirements.Keys)
                    AddAspectToDisplay(k, requirements[k]);
        }

        private void AddAspectToDisplay(string aspectId, string criterion)
        {
            Element aspectElement = Watchman.Get<Compendium>().GetEntityById<Element>(aspectId);

            
            ElementFrame newElementFrame = Watchman.Get<PrefabFactory>().CreateLocally<ElementFrame>(transform);
            newElementFrame.PopulateDisplay(aspectElement, criterion, false);
            

            }

        public virtual void ClearCurrentlyDisplayedRequirements()
        {
            foreach (ElementFrame a in GetComponentsInChildren<ElementFrame>())
                Object.Destroy(a.gameObject);
        }

    }
}
