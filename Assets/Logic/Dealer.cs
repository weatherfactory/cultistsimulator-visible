using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Noon;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Logic
{
    public class Dealer
    {
        private Character _character;
        public Dealer(Character character)
        {
            _character = character;
        }
        //if passed an element id, return it;
        //if passed a deck id, return a (recursive) draw from that deck
        public string Deal(DeckInstance deck)
        {
            var drawnId = deck.Draw();
            if (drawnId.StartsWith(NoonConstants.DECK_PREFIX))
            {
                var deckId = drawnId.Replace(NoonConstants.DECK_PREFIX, "");
                var subDeck = _character.GetDeckInstanceById(deckId);
                return Deal(subDeck);
            }
            else
                return drawnId;
        }



        public DrawWithMessage DealWithMessage(DeckInstance deck)
        {
            var drawnCard = Deal(deck);
            var drawWithMessage = new DrawWithMessage {DrawnCard = drawnCard};
            if (deck.GetDrawMessages().ContainsKey(drawnCard))
                drawWithMessage.WithMessage = deck.GetDrawMessages()[drawnCard];
            else
            {
                if (deck.GetDefaultDrawMessages().Any())
                   drawWithMessage.WithMessage=deck.GetDefaultDrawMessages().OrderBy(x => Random.value).First().Value;
            }

            return drawWithMessage;
        }



        public void IndicateUniqueCardManifested(string elementId)
        {
            foreach (var d in _character.GetAllDecks())
            { 
                d.EliminateCardWithId(elementId);
            }

        }

        public void RemoveFromAllDecksIfInUniquenessGroup(string elementUniquenessGroup)
        {
            foreach (var d in _character.GetAllDecks())
            {
                d.EliminateCardsInUniquenessGroup(elementUniquenessGroup);
            }
        }
    }
}
