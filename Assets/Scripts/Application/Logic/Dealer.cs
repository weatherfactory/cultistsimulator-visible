using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;
using SecretHistories.Spheres;
using SecretHistories.UI;

using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Logic
{
    public class Dealer
    {
        private readonly IHasCardPiles _dealersTable;
        private readonly System.Random _r;
        public Dealer(IHasCardPiles dealersTable)
        {
            _dealersTable = dealersTable;
            _r = new System.Random();

        }


        public Token Deal(string fromDeckSpecId)
        {
            var deckSpec=  Watchman.Get<Compendium>().GetEntityById<DeckSpec>(fromDeckSpecId);
            return Deal(deckSpec);
        }

        public Token Deal(DeckSpec fromDeckSpec)
        {
            Token cardDrawn;
            var _drawPile = _dealersTable.GetDrawPile(fromDeckSpec.Id);
            var _forbiddenPile = _dealersTable.GetForbiddenPile(fromDeckSpec.Id);
            

            if (_drawPile.GetTotalStacksCount() == 0)
                Shuffle(fromDeckSpec);
            
            //shuffling the might still not have given us a card - eg if all cards were forbidden.
            //so let's test again
            if (_drawPile.GetTotalStacksCount() > 0)
            {
                cardDrawn = _drawPile.GetElementTokens().First();

                if(!fromDeckSpec.ResetOnExhaustion)
                    ForbidRedrawOfCard(cardDrawn.Payload.EntityId, fromDeckSpec);
                //This means that a 'don't reset on exhaustion' deck will forbid every card we draw, so a second shuffle won't renew anything.

            }

            else
            {
                //if either the deck didn't reset on exhaustion,
                //or a reset has still left us with no cards,
                //then always return the default card
                cardDrawn= _drawPile.ProvisionElementToken(fromDeckSpec.DefaultCard, 1);
            }

            //if we can find a suitable draw message, set an illumination
            if (fromDeckSpec.DrawMessages.ContainsKey(cardDrawn.PayloadEntityId))
            {
                string drawMessage = fromDeckSpec.DrawMessages.First(m => m.Key == cardDrawn.PayloadEntityId).Value;
                cardDrawn.Payload.SetIllumination(NoonConstants.MESSAGE_ILLUMINATION_KEY,drawMessage);
            }

            return cardDrawn;


        }


        public void Shuffle(string fromDeckSpecId)
        {
            var deckSpec = Watchman.Get<Compendium>().GetEntityById<DeckSpec>(fromDeckSpecId);
            Shuffle(deckSpec);
        }

        public void Shuffle(DeckSpec fromDeckSpec)
        {
            
            var drawPile = _dealersTable.GetDrawPile(fromDeckSpec.Id);
            var forbiddenPile = _dealersTable.GetForbiddenPile(fromDeckSpec.Id);
        
            var forbiddenCards = forbiddenPile.GetElementTokens().Select(f => f.PayloadEntityId);
            var permittedCards = fromDeckSpec.Spec.Except(forbiddenCards);

            NoonUtility.Log($"Shuffling {fromDeckSpec.Id}. {permittedCards.Count()} cards available to shuffle.");

            foreach (var card in permittedCards.OrderBy(x=>_r.Next()))
            {
                drawPile.ProvisionElementToken(card, 1);
            }
        }

        /// <summary>
        /// This card is unique and has been drawn elsewhere, or belongs to the same uniqueness group as one that has been drawn elsewhere
        /// </summary>
        /// <param name="elementId"></param>
        public void IndicateUniqueElementManifested(string elementId)
        {
            NoonUtility.Log($"Unique element manifested: {elementId}");


            foreach (var d in _dealersTable.GetDrawPiles())
            { 
                d.RetireTokensWhere(x => x.Payload.EntityId == elementId);

                //REFACTOR: I do in fact need to do both of these... what happens if Ezeem has been drawn in the past, and then is shuffled back in later?
                //REFACTOR: more elegant alternative might be to add the cards to the forbidden pile, and then autoremove all instances of that card from the matching draw pile whenever a card enters the forbidden pile

                var deckSpec = Watchman.Get<Compendium>().GetEntityById<DeckSpec>(d.GetDeckSpecId());

                if (deckSpec.Spec.Contains(elementId))
                {
                    NoonUtility.Log($"Forbidding redraw of {elementId} from {deckSpec.Id}");
                    ForbidRedrawOfCard(elementId, deckSpec);
                }
            }

        }

        public void IndicateElementInUniquenessGroupManifested(string entityId,string elementUniquenessGroup)
        {
            NoonUtility.Log($"Unique element manifested: {entityId} from {elementUniquenessGroup}");

            var elementsToEliminate = Watchman.Get<Compendium>().GetEntitiesAsList<Element>()
                .Where(e => e.UniquenessGroup == elementUniquenessGroup);

            foreach (var element in elementsToEliminate)
            {
                IndicateUniqueElementManifested(element.Id);
            }
        }


        private void ForbidRedrawOfCard(string elementId, DeckSpec deckSpec)
        {
            var forbiddenPile = _dealersTable.GetForbiddenPile(deckSpec.Id);
            if(forbiddenPile.GetElementTokens().All(t => t.Payload.EntityId != elementId)) //don't clutter the forbidden pile unless there's no such card in there already
            {
                forbiddenPile.ProvisionElementToken(elementId, 1);

            }
        }


    }
}
