using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

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
            if (drawnId.StartsWith("deck:"))
            {
                var deckId = drawnId.Replace("deck:", "");
                var subDeck = _storage.GetDeckInstanceById(deckId);
                return Deal(subDeck);
            }
            else
                return drawnId;
        }
    }
}
