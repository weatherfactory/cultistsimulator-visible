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


           string linkProperties=string.Empty;
           string chance=string.Empty;

           if (!details.Challenges.Any())
               chance = details.Chance.ToString();
           else
           {
               foreach (var challenge in details.Challenges)
               {
                   if (string.IsNullOrEmpty(chance))
                       chance += ",";
                   chance += $"{challenge.Key}:{challenge.Value}";
               }
           }
           if (details.Additional)
               linkProperties = $"({chance} +)";
           else
           {
               linkProperties = $"({chance})";
           }

           string descriptions = $"{TrimToMaxOrLess(r.StartDescription)}/{TrimToMaxOrLess(r.Description)}";
           

           _summary.text = $"{linkProperties} <b>{details.Id}<b>: {descriptions}";
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
