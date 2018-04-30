using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Noon;

namespace Assets.Logic
{
    public class Dealer
    {
        private IGameEntityStorage _storage;
        public Dealer(IGameEntityStorage storage)
        {
            _storage = storage;
        }
        //if passed an element id, return it;
        //if passed a deck id, return a (recursive) draw from that deck
        public string Deal(IDeckInstance deck)
        {
            var drawnId = deck.Draw();
            if (drawnId.StartsWith(NoonConstants.DECK_PREFIX))
            {
                var deckId = drawnId.Replace(NoonConstants.DECK_PREFIX, "");
                var subDeck = _storage.GetDeckInstanceById(deckId);
                return Deal(subDeck);
            }
            else
                return drawnId;
        }

        public void RemoveFromAllDecks(string elementId)
        {
            foreach (var d in _storage.DeckInstances)
                d.RemoveAllCardsWithId(elementId);
                

        }
    }
}
