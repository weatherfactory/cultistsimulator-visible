using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.UI;
using TMPro;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Meta
{
   public class LinkedRecipeDetailsDisplay: MonoBehaviour
   {
       [SerializeField] private TextMeshProUGUI _summary;

       const int maxStringLength = 20;
       private const string trimmedMarker = "...";
        public void Populate(LinkedRecipeDetails details)
       {
           
           var r = Watchman.Get<Compendium>().GetEntityById<Recipe>(details.Id);

            
           _summary.text = $"({details.Chance}) <b>{details.Id}<b>: {TrimToMaxOrLess(r.StartDescription)}/{TrimToMaxOrLess(r.Description)}";
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
