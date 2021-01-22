﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using UnityEngine;

namespace Assets.Scripts.Application.UI.Situation
{
   public class SituationDeckEffectsView: MonoBehaviour,ISituationSubscriber
    {
        [SerializeField] DeckEffectView[] deckEffectViews;
        public void SituationStateChanged(SecretHistories.Entities.Situation situation)
        {
            ShowDeckEffects(situation.CurrentPrimaryRecipe.DeckEffects);
        }

        public void TimerValuesChanged(SecretHistories.Entities.Situation s)
        {
        //    throw new NotImplementedException();
        }

        public void SituationSphereContentsUpdated(SecretHistories.Entities.Situation s)
        {
          //  throw new NotImplementedException();
        }

        public void ReceiveNotification(INotification n)
        {
        //    throw new NotImplementedException();
        }

        private void ShowDeckEffects(Dictionary<string, int> deckEffects)
        {
            if (deckEffects.Count > deckEffectViews.Length)
                NoonUtility.LogWarning($"{deckEffects.Count} deck effects to show in OngoingDisplay, but only {deckEffectViews.Length} slots.");

            int i = 0;
            foreach (var dev in deckEffectViews)
                dev.gameObject.SetActive(false);


            // Populate those we need
            foreach (var item in deckEffects)
            {
                var deckSpec = Watchman.Get<Compendium>().GetEntityById<DeckSpec>(item.Key);
                deckEffectViews[i].PopulateDisplay(deckSpec, item.Value);
                deckEffectViews[i].gameObject.SetActive(true);
                i++;
            }


        }
    }
}