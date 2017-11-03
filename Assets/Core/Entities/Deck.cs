using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Noon;

namespace Assets.Core.Entities
{

    /// <summary>
    /// A Deck is a wrapped stack in the classic, not the Cultist Simulator-specific, sense.
    /// I'm wrapping it partly to encapsulate implementation but also to minimise naming confusion.
    /// </summary>
    public class Deck
    {
        private string _id;
        //Deck spec determines which cards start in the deck after each reset
        private List<string> _deckSpec;
        //cards determines which cards are currently in the deck
        private Stack<string> _cards;

        public Deck(string id,List<string> deckSpec)
        {
            _id = id;
            _deckSpec = deckSpec;
            _cards=new Stack<string>();
        }

        /// <summary>
        /// resets deck (with up to date version of each stack). Use this when first creating the deck
        /// </summary>
        public void Reset()
        {
           var rnd=new Random();
            var unshuffledStack=new Stack<string>();
            foreach (var eId in _deckSpec)
            {
                Add(eId);
            }

            _cards=new Stack<string>(unshuffledStack.OrderBy(x=>rnd.Next()));
        }

        public string Id
        {
            get { return _id; }
        }

        public string Draw()
        {
            return _cards.Pop();
        }

        

        public void Add(string elementId)
        {
            _cards.Push(elementId);
        }
    }
}
