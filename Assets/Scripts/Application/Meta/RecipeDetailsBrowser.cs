using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Services;
using SecretHistories.UI;
using TMPro;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Meta
{
   public class RecipeDetailsBrowser: MonoBehaviour
   {
       [SerializeField] private GameObject linksContainer;


       public void Clear()
       {
           PopulateLinks(new List<LinkedRecipeDetails>());
       }

        public void PopulateLinks(List<LinkedRecipeDetails> links)
        {
         foreach(Transform c in linksContainer.transform)
             GameObject.Destroy(c.gameObject);

         foreach (var l in links)
            {
                var prefabLinkDisplay = Watchman.Get<PrefabFactory>().CreateLocally<LinkedRecipeDetailsDisplay>(linksContainer.transform);
                prefabLinkDisplay.name = l.Id;
                prefabLinkDisplay.Populate(l);
            }
        }
    }
}
