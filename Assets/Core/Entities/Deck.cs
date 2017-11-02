using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;

namespace Assets.Core.Entities
{
    /// <summary>
    /// A Deck is a wrapped stack in the classic, not the Cultist Simulator-specific, sense.
    /// I'm wrapping it partly to encapsulate implementation but also to minimise naming confusion.
    /// </summary>
    public class Deck
    {
        private string _id;
        private Stack<IElementStack> _cards;

        public Deck(string id)
        {
            _id = id;
            _cards = new Stack<IElementStack>();
        }

        public Deck(string id,Stack<IElementStack> cards)
        {
            _id = id;
            cards = new Stack<IElementStack>();
        }

        public string Id
        {
            get { return _id; }
        }

        public IElementStack Draw()
        {
            return _cards.Pop();
        }

        public void Add(IElementStack card)
        {
            _cards.Push(card);
        }
    }
}
