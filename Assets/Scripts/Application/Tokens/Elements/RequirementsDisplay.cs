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

        public void DisplayRequirementsAndFulfilments(Recipe r, Situation situation)
        {
            var aspectsInContext =
                Watchman.Get<HornedAxe>().GetAspectsInContext(situation.GetAspects(true));

            
            foreach (var req in r.Requirements)
            {
                if (Recipe.CheckRequirementsSatisfiedForContext(aspectsInContext.AspectsInSituation, req))
                    DisplayMatchedRequirement(req);
                else
                    DisplayUnmatchedRequirement(req);
            }

            if(r.TableReqs.Any())
                AddFilterLabel("TAB");

            foreach (var treq in r.TableReqs)
            {
                if (Recipe.CheckRequirementsSatisfiedForContext(aspectsInContext.AspectsOnTable, treq))
                    DisplayMatchedRequirement(treq);
                else
                    DisplayUnmatchedRequirement(treq);
            }

            if(r.ExtantReqs.Any())
                AddFilterLabel("EXT");

            foreach (var ereq in r.ExtantReqs)
            {
                if (Recipe.CheckRequirementsSatisfiedForContext(aspectsInContext.AspectsExtant, ereq))
                    DisplayMatchedRequirement(ereq);
                else
                    DisplayUnmatchedRequirement(ereq);
            }




        }

        private void DisplayMatchedRequirement(KeyValuePair<string, string> req)
        {
            DisplayUnmatchedRequirement(req);
        }

        private void DisplayUnmatchedRequirement(KeyValuePair<string, string> req)
        {
            Element reqelement = Watchman.Get<Compendium>().GetEntityById<Element>(req.Key);

            ReqFrame newReqFrame = Watchman.Get<PrefabFactory>().CreateLocally<ReqFrame>(transform);
          newReqFrame.Populate(reqelement, req.Value);
        }


        private void AddFilterLabel(string filter)
        {
            var filterLabelObj = new GameObject();
            filterLabelObj.name = $"{filter}_label";
            var filterLabelText = filterLabelObj.AddComponent<TextMeshProUGUI>();
            filterLabelObj.AddComponent<LayoutElement>();
            filterLabelText.text = filter;
            filterLabelText.fontSize = 12f;
            filterLabelObj.transform.SetParent(gameObject.transform, false);
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
