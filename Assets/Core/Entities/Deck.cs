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
        private Stack<IElementStack> cards;

        public Deck()
        {
            cards=new Stack<IElementStack>();
        }

        public Deck(Stack<IElementStack> cards)
        {
            cards = new Stack<IElementStack>();
        }

        public IElementStack Draw()
        {
            return cards.Pop();
        }

        public void Add(IElementStack card)
        {
            cards.Push(card);
        }
    }
}
