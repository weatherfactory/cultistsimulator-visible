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


namespace SecretHistories
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
                    DisplayRequirement(req,true);
                else
                    DisplayRequirement(req, false);
            }

            if (r.RoomReqs.Any())
                AddFilterLabel("ROO");

            foreach (var rreq in r.RoomReqs)
            {
                if (Recipe.CheckRequirementsSatisfiedForContext(aspectsInContext.AspectsOnTable, rreq))
                    DisplayRequirement(rreq, true);
                else
                    DisplayRequirement(rreq, false);
            }


            if (r.TableReqs.Any())
                AddFilterLabel("TAB");

            foreach (var treq in r.TableReqs)
            {
                if (Recipe.CheckRequirementsSatisfiedForContext(aspectsInContext.AspectsOnTable, treq))
                    DisplayRequirement(treq, true);
                else
                    DisplayRequirement(treq, false);
            }

            if(r.ExtantReqs.Any())
                AddFilterLabel("EXT");

            foreach (var ereq in r.ExtantReqs)
            {
                if (Recipe.CheckRequirementsSatisfiedForContext(aspectsInContext.AspectsExtant, ereq))
                    DisplayRequirement(ereq, true);
                else
                    DisplayRequirement(ereq,false);
            }




        }


        private void DisplayRequirement(KeyValuePair<string, string> req,bool matched)
        {
            Element reqelement = Watchman.Get<Compendium>().GetEntityById<Element>(req.Key);
            ReqFrame newReqFrame = Watchman.Get<PrefabFactory>().CreateLocally<ReqFrame>(transform);
            newReqFrame.Populate(reqelement, req.Value,matched);
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
