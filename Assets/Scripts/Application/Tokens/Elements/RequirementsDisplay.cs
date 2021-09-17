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
using UnityEngine.UI;


namespace SecretHistories.Assets.Scripts.Application.Tokens.Elements
{
    public class RequirementsDisplay : MonoBehaviour
    {

        public virtual void DisplayRequirements(Dictionary<string, string> requirements, string filter)
        {
            AddFilterLabel(filter);
            foreach (string k in requirements.Keys)
                AddAspectToDisplay(k, requirements[k], filter);
        }

        private void AddFilterLabel(string filter)
        {
            var filterLabelObj = new GameObject();
            filterLabelObj.name = $"{filter}_label";
            var filterLabel = filterLabelObj.AddComponent<TextMeshProUGUI>();
            filterLabel.text = filter;
            filterLabel.fontSize = 12f;
            filterLabelObj.transform.SetParent(gameObject.transform, false);
        }


        private void AddAspectToDisplay(string aspectId, string criterion, string filter)
        {
            Element aspectElement = Watchman.Get<Compendium>().GetEntityById<Element>(aspectId);


            ElementFrame newElementFrame = Watchman.Get<PrefabFactory>().CreateLocally<ElementFrame>(transform);
            newElementFrame.PopulateDisplay(aspectElement, criterion, false);


        }


        public virtual void ClearCurrentlyDisplayedRequirements()
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }

        }
    }
}
