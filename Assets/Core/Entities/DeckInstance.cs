using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.TokenContainers;
using Noon;
using UnityEngine;
using Random = System.Random;

namespace Assets.Core.Entities
{
    public class DeckInstance : MonoBehaviour, ISaveable
    {
        private DeckSpec _deckSpec;
      [SerializeField]  private CardsPile _drawPile;
      [SerializeField] private CardsPile _forbiddenCards;
        


      public string Id => _deckSpec?.Id;


        /// <summary>
        /// Resets with a deckspec, but *does* not yet populate the cards list
        /// </summary>
        /// <param name="spec"></param>
        public void SetSpec(DeckSpec spec)
        {

            if (spec == null)
                throw new ApplicationException("Can't initialise a deckinstance with a null deckspec");

            name = spec.Id;

            _deckSpec = spec;
            _drawPile.SetSpec(spec);
        }


        public void Shuffle()
        {
        _drawPile.Shuffle(_forbiddenCards.GetUniqueStackElementIds());
        }


        public string Draw()
        {
            if (_drawPile.GetTotalStacksCount()==0)
            {
                //if the deck is exhausted:
                //--some decks reset, so we can have an infinite supply of whatevers.
                if (_deckSpec.ResetOnExhaustion)
                    Shuffle();
            }
            //Conceivably, resetting the deck might still not have given us a card,
            //so let's test again
            if (_drawPile.GetTotalStacksCount() > 0)
            {
                var cardDrawn = _drawPile.GetStacks().First();
                return cardDrawn.EntityId;
            }

            
            else
                //if either the deck didn't reset on exhaustion,
                //or a reset has still left us with no cards, always return the default card
                return _deckSpec.DefaultCard;
        }


        public void Add(string elementId)
        {
            _drawPile.ProvisionElementStack(elementId, 1, Source.Existing(), new Context(Context.ActionSource.Unknown));
        }
        /// <summary>
        /// This card is unique and has been drawn elsewhere, or belongs to the same uniqueness group as one that has been drawn elsewhere
        /// </summary>
        /// <param name="elementId"></param>
        public void EliminateCardWithId(string elementId)
        {
            _drawPile.RetireWith(x=>x.EntityId==elementId);

            if (_deckSpec.Spec.Contains(elementId))
                _forbiddenCards.ProvisionElementStack(elementId, 1);

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
            return new List<string>(_drawPile.GetStackElementIds());
            
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
            foreach (var e in _forbiddenCards.GetUniqueStackElementIds())
            {
                alEliminatedCards.Add(e);
            }
            cardsHashtable.Add(SaveConstants.SAVE_ELIMINATEDCARDS,alEliminatedCards);

            return cardsHashtable;
        }

    }
}
