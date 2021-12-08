using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Entities;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;

namespace Assets.Scripts.Application.UI.Situation
{
   public class SituationDeckEffectsView: MonoBehaviour,ISituationSubscriber
    {
        [SerializeField] DeckEffectView[] deckEffectViews;
        public void SituationStateChanged(SecretHistories.Entities.Situation situation)
        {
            ShowDeckEffects(situation.Recipe.DeckEffects);
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
            foreach (var effect in deckEffects)
            {
                var deckSpec = Watchman.Get<Compendium>().GetEntityById<DeckSpec>(effect.Key);

                DeckEffect deckEffect = new DeckEffect(deckSpec, effect.Value);

                deckEffectViews[i].PopulateDisplay(deckEffect);
                deckEffectViews[i].gameObject.SetActive(true);
                i++;
            }


        }
    }
}
