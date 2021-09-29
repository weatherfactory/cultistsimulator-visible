using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Tokens.Elements;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums.Elements;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.Assets.Scripts.Application.Meta
{
    //to display exactly one linkedrecipedetails
   public class LinkedRecipeDetailsDisplay: MonoBehaviour
   {
#pragma warning disable 649
       [SerializeField] private RequirementsDisplay _requirements;
       [SerializeField] private Button _slotMarkerButton;
       [SerializeField] private TextMeshProUGUI _additional;
       [SerializeField] private TextMeshProUGUI _summary;
       

       
#pragma warning restore 649

       const int maxStringLength = 20;
       private const string trimmedMarker = "...";
        public void Populate(LinkedRecipeDetails details,Situation situation)
       {

           _requirements.ClearCurrentlyDisplayedRequirements();

            var r = Watchman.Get<Compendium>().GetEntityById<Recipe>(details.Id);
            _requirements.DisplayRequirementsAndFulfilments(r,situation);

            


            //if (r.RequirementsSatisfiedBy(aspectsInContext))
            //     _showEligibility.Show();
            //else
            //    _showEligibility.Hide();

            string linkProperties=String.Empty;
           string possibilityDescription=string.Empty;

           if (!details.Challenges.Any() && details.Chance>0)
               possibilityDescription = details.Chance.ToString();
           else
           {
               foreach (var challenge in details.Challenges)
               {
                   if (string.IsNullOrEmpty(possibilityDescription))
                       possibilityDescription += ",";
                   possibilityDescription += $"{challenge.Key}:{challenge.Value}";
               }
           }

           if (!string.IsNullOrEmpty(possibilityDescription))
               linkProperties += $"({possibilityDescription})";

          

           var descriptions = $"{TrimToMaxOrLess(r.StartDescription)}/{TrimToMaxOrLess(r.Description)}";
           

           _summary.text = $"{linkProperties} <b>{details.Id}<b>: {descriptions}";

           if (details.Additional)
               _additional.gameObject.SetActive(true);
           else
               _additional.gameObject.SetActive(false);


           if (r.Slots.Any())
           {
               _slotMarkerButton.gameObject.SetActive(true);
               _slotMarkerButton.onClick.AddListener(delegate { ShowSlotDetails(r.Slots.First()); });

           }
           else
           {
               _slotMarkerButton.onClick.RemoveAllListeners();
                _slotMarkerButton.gameObject.SetActive(false);
           }
        }

        private void ShowSlotDetails(SphereSpec sphereSpec)
        {
            Watchman.Get<Notifier>().ShowSlotDetails(sphereSpec);
        }

        private string TrimToMaxOrLess(string toTrim)
        {
            if (toTrim.Length < maxStringLength)
                return toTrim;

            int maxWithMarker = maxStringLength - trimmedMarker.Length;

            return $"{toTrim.Substring(0, maxWithMarker)}{trimmedMarker}";

        }
   }
}
