using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Noon;

namespace Assets.Core.Entities
{
    public interface IDeckSpec
    {
        /// <summary>
        /// resets deckSpec (with up to date version of each stack). Use this when first creating the deckSpec
        /// </summary>

        string Id { get; }
        List<string> StartingCards { get; set; }

    }

    
    public interface IDeckInstance:ISaveable
    {
        /// <summary>
        /// resets deckSpec (with up to date version of each stack). Use this when first creating the deckSpec
        /// </summary>
        void Reset();

        string Id { get; }
        string Draw();
        void Add(string elementId);
        List<string> GetCurrentCardsAsList();
    }


 public class DeckSpec : IDeckSpec
    {
        private string _id;
        //DeckSpec startingCards determines which cards start in the deckSpec after each reset
        public List<string> StartingCards { get; set; }
        public string DefaultCardId { get; set; }

        public DeckSpec(string id,List<string> startingCards,string defaultCardId)
        {
            _id = id;
            StartingCards = startingCards;
            DefaultCardId = defaultCardId;
        }


        /// <summary>
        /// resets deckSpec (with up to date version of each stack). Use this when first creating the deckSpec
        /// </summary>

        public string Id
        {
            get { return _id; }
        }





    }

    public class DeckInstance:IDeckInstance
    {
        private IDeckSpec _deckSpec;
        private Stack<string> _cards;

        public string Id
        {
            get { return _deckSpec.Id; }
        }

        public DeckInstance(IDeckSpec spec)
        {
            if (spec == null)
                throw new ApplicationException("Can't initialise a deckinstance with a null deckspec");

            _deckSpec = spec;
            _cards=new Stack<string>();
            
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
            if (_cards.Any())
                return _cards.Pop();
            else
                return null;


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
