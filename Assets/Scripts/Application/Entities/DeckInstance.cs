using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Infrastructure;
using SecretHistories.TokenContainers;

using UnityEngine;
using Random = System.Random;

namespace SecretHistories.Entities
{
    public class DeckInstance : MonoBehaviour
    {
#pragma warning disable 649
        private DeckSpec _deckSpec;
      [SerializeField]  private CardsPile _drawPile;
      [SerializeField] private CardsPile _forbiddenCards;
      public string Id => _deckSpec?.Id;

#pragma warning restore 649
        /// <summary>
        /// Resets with a deckspec, but *does* not yet populate the cards list
        /// </summary>
        /// <param name="spec"></param>
        public void SetSpec(DeckSpec spec)
        {

            if (spec == null)
                throw new ApplicationException("Can't initialise a deckinstance with a null deckspec");

            name = spec.Id;

            _deckSpec = spec;
            _drawPile.SetSpec(spec);
        }


        public void Shuffle()
        {
        _drawPile.Shuffle(_forbiddenCards.GetUniqueStackElementIds());
        }


        public string Draw()
        {
            if (_drawPile.GetTotalStacksCount()==0)
            {
                //if the deck is exhausted:
                //--some decks reset, so we can have an infinite supply of whatevers.
                if (_deckSpec.ResetOnExhaustion)
                    Shuffle();
            }
            //Conceivably, resetting the deck might still not have given us a card,
            //so let's test again
            if (_drawPile.GetTotalStacksCount() > 0)
            {
                var cardDrawn = _drawPile.GetElementTokens().First();
                return cardDrawn.Element.Id;
            }

            
            else
                //if either the deck didn't reset on exhaustion,
                //or a reset has still left us with no cards, always return the default card
                return _deckSpec.DefaultCard;
        }


        public void Add(string elementId)
        {
            _drawPile.ProvisionElementStackToken(elementId, 1, Source.Existing(), new Context(Context.ActionSource.Unknown));
        }
        /// <summary>
        /// This card is unique and has been drawn elsewhere, or belongs to the same uniqueness group as one that has been drawn elsewhere
        /// </summary>
        /// <param name="elementId"></param>
        public void EliminateCardWithId(string elementId)
        {
            _drawPile.RetireTokensWhere(x=>x.Element.Id==elementId);

            if (_deckSpec.Spec.Contains(elementId))
                _forbiddenCards.ProvisionElementStackToken(elementId, 1);

        }

        public void EliminateCardsInUniquenessGroup(string elementUniquenessGroup)
        {
            List<string> cardsToEliminate = _deckSpec.CardsInUniquenessGroup(elementUniquenessGroup);
            if(cardsToEliminate!=null)
                foreach(var c in cardsToEliminate)
                    EliminateCardWithId(c);
        }


        public List<string> GetCurrentCardsAsList()
        {
            return new List<string>(_drawPile.GetStackElementIds());
            
        }


        public Dictionary<string, string> GetDefaultDrawMessages()
        {
            return new Dictionary<string, string>(_deckSpec.DefaultDrawMessages);
        }

        public Dictionary<string, string> GetDrawMessages()
        {
            return new Dictionary<string, string>(_deckSpec.DrawMessages);
        }


    }
}
