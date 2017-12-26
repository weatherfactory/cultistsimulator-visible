using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    public class DeckInstance : IDeckInstance
    {
        private IDeckSpec _deckSpec;
        private Stack<string> _cards;

        public string Id
        {
            get { return _deckSpec.Id; }
        }

        protected DeckInstance()
        {
            
        }

        public DeckInstance(IDeckSpec spec)
        {
            if (spec == null)
                throw new ApplicationException("Can't initialise a deckinstance with a null deckspec");

            _deckSpec = spec;
            _cards = new Stack<string>();

        }

        public void Reset()
        {
            var rnd = new Random();
            var unshuffledStack = new Stack<string>();
            foreach (var eId in _deckSpec.StartingCards)
            {
                unshuffledStack.Push(eId);
            }

            _cards = new Stack<string>(unshuffledStack.OrderBy(x => rnd.Next()));
        }



        public string Draw()
        {
            if (!_cards.Any())
            {
                //if the deck is exhausted:
                //--some decks reset, so we can have an infinite supply of whatevers.
                if (_deckSpec.ResetOnExhaustion)
                    Reset();
            }
            //in case somehow resetting the deck still won't have given us a card,
            //let's test again
            if(_cards.Any())
            return _cards.Pop();
            else
                //if either the deck didn't reset on exhaustion,
                //or a reset has still left us with no cards, always return the default card
                return _deckSpec.DefaultCardId;
        }


        public void Add(string elementId)
        {
            _cards.Push(elementId);
        }


        public List<string> GetCurrentCardsAsList()
        {
            var cardsList = new List<string>(_cards);
            cardsList.Reverse(); //it's a stack, so it goes from the top down
            return cardsList;
        }

        public Hashtable GetSaveData()
        {
            var cardsHashtable = new Hashtable();
            foreach (var c in GetCurrentCardsAsList())
            {
                var indexForTable = (cardsHashtable.Count + 1).ToString();
                cardsHashtable.Add(indexForTable, c);
            }
            return cardsHashtable;
        }
    }
}
