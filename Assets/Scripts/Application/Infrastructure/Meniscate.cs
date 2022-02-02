#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Services;
using Assets.Logic;
using SecretHistories.Constants;
using SecretHistories.UI;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;

using SecretHistories.Enums.Elements;
using SecretHistories.Infrastructure;
using SecretHistories.NullObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

namespace SecretHistories.UI {
    public class Meniscate : MonoBehaviour
    {

        [SerializeField] private CardHoverDetail cardHoverDetail;


        public void Awake()
        {
            var registry = new Watchman();
            registry.Register(this);
        }

        public void Update()
        {
            Watchman.Get<Concursum>().DoUpdate();
        }



        public void CloseAllSituationWindowsExcept(string exceptVerbId) {
            var situations = Watchman.Get<HornedAxe>().GetRegisteredSituations();

            foreach (var s in situations)
            {
                if (s.Verb.Id != exceptVerbId)
                    s.Close();
            }
        }

        public bool IsSituationWindowOpen() {
	        var situations = Watchman.Get<HornedAxe>().GetRegisteredSituations();
	        return situations.Any(c => c.IsOpen);
        }

        public Situation GetCurrentlyOpenSituation()
        {
            var situations = Watchman.Get<HornedAxe>().GetRegisteredSituations();
            var openSituations = situations.Where(s => s.IsOpen);

            
            var enumerable = openSituations.ToList();
            if (enumerable.Count > 1)
            {
               string warning=$"Found {situations.Count} open situations, which shouldn't happen: ";
               foreach (var o in new List<Situation>(enumerable))
                   warning += $" {o.Id};";
               NoonUtility.LogWarning(warning);

               return enumerable.First();
            }

            if (enumerable.Count == 0)
                return NullSituation.Create();

            return enumerable.Single();

        }

        public void SetHighlightedElement(string elementId, int quantity = 1)
        {
            var enableAccessibleCards =
                Watchman.Get<Config>().GetConfigValueAsInt(NoonConstants.ACCESSIBLECARDS);

            if (enableAccessibleCards==null || enableAccessibleCards==0)
		        return;

            if (cardHoverDetail == null) //in BH, there's no CardHoverDetail reference set yet.
                return;
            
            if (elementId == null)
	        {
		        cardHoverDetail.Hide();
		        return;
	        }
	        cardHoverDetail.Populate(elementId, quantity);
	        cardHoverDetail.Show();
        }


    }


}
