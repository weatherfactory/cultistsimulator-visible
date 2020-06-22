using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;

namespace Assets.Core.Entities
{
    public class DeckInstance : IDeckInstance
    {
        private IDeckSpec _deckSpec;
        private Stack<string> _cards;
        private List<string> _eliminatedCards;

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
            _eliminatedCards=new List<string>();

        }


        public void Reset()
        {
            var rnd = new Random();
            var unshuffledStack = new Stack<string>();
            foreach (var eId in _deckSpec.Spec)
            {
                if(!_eliminatedCards.Contains(eId))
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
            //Conceivably, resetting the deck might still not have given us a card,
            //so let's test again
            if (_cards.Any())
            {
                var result = _cards.First();
                //decks can contain subdecks. If this is a subdeck,don't pop the result, just return it - but do shuffle the deck, so
                //we don't keep getting the same result (unless it's the last one anyway)
                //btw, this does mean that subdeck reset / default settings take precedence over top deck ones.
                if (result.Contains(NoonConstants.DECK_PREFIX))
                {
                    var rnd = new Random();
                    _cards = new Stack<string>(_cards.OrderBy(x => rnd.Next()));
                    return result;
                }
                else
                    return _cards.Pop();

            }

            
            else
                //if either the deck didn't reset on exhaustion,
                //or a reset has still left us with no cards, always return the default card
                return _deckSpec.DefaultCard;
        }


        public void Add(string elementId)
        {
            _cards.Push(elementId);
        }
        /// <summary>
        /// This card is unique and has been drawn elsewhere, or belongs to the same uniqueness group as one that has been drawn elsewhere
        /// </summary>
        /// <param name="elementId"></param>
        public void EliminateCardWithId(string elementId)
        {
            var cardsList = new List<string>(_cards);
            if(cardsList.Contains(elementId))
            {
                RemoveCardFromDeckInstance(elementId, cardsList);
            }

            if(_deckSpec.Spec.Contains(elementId))
                TryAddToEliminatedCardsList(elementId); //if the card isn't in the list, it's either (a) already been drawn or (b) isn't in the deck to begin with. If it's already been drawn, then it itself should be the sole non-eliminated card.

        }
        ///remove this from the undrawn cards. This won't affect default draws.
        private void RemoveCardFromDeckInstance(string elementId, List<string> cardsList)
        {
            NoonUtility.Log("Removing " + elementId + " from " + _deckSpec.Id);
            cardsList.RemoveAll(c => c == elementId);
            _cards = new Stack<string>(cardsList);
        }
        /// <summary>
        /// add this to a list of permanently eliminated cards, so it doesn't appear on reshuffles.
        /// </summary>
        /// <param name="elementId"></param>
        public  void TryAddToEliminatedCardsList(string elementId)
        {
            if(!_eliminatedCards.Contains(elementId))
                _eliminatedCards.Add(elementId);
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
            var cardsList = new List<string>(_cards);
            cardsList.Reverse(); //it's a stack, so it goes from the top down
            return cardsList;
        }


        public Dictionary<string, string> GetDefaultDrawMessages()
        {
            return new Dictionary<string, string>(_deckSpec.DefaultDrawMessages);
        }

        public Dictionary<string, string> GetDrawMessages()
        {
            return new Dictionary<string, string>(_deckSpec.DrawMessages);
        }

        public Hashtable GetSaveData()
        {
            var cardsHashtable = new Hashtable();
       

            foreach (var c in GetCurrentCardsAsList())
            {
                var indexForTable = (cardsHashtable.Count + 1).ToString();
                cardsHashtable.Add(indexForTable, c);
            }

            var alEliminatedCards=new ArrayList();
            foreach (var e in _eliminatedCards)
            {
                alEliminatedCards.Add(e);
            }
            cardsHashtable.Add(SaveConstants.SAVE_ELIMINATEDCARDS,alEliminatedCards);

            return cardsHashtable;
        }
    }
}
