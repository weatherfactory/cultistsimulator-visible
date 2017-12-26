using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;

namespace Assets.Logic
{
    public class DeckEffectExecutor
    {
        private IGameEntityStorage _storage;
        public DeckEffectExecutor(IGameEntityStorage storage)
        {
            _storage = storage;
        }
        //if passed an element id, return it;
        //if passed a deck id, return a (recursive) draw from that deck
        public string ElementOrDeckResult(string id)
        {
            throw new NotImplementedException();
        }
    }
}
