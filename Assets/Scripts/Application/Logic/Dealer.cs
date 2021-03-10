using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Commands;
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
        private readonly DealersTable _dealersTable;

        public Dealer(DealersTable dealersTable)
        {
            _dealersTable = dealersTable;
        }

        //public DrawWithMessage DealWithMessage(DeckInstance deck)
        //{
        //    var drawnCard = Deal(deck.Id);
        //    var drawWithMessage = new DrawWithMessage {DrawnCard = drawnCard};
        //    if (deck.GetDrawMessages().ContainsKey(drawnCard))
        //        drawWithMessage.WithMessage = deck.GetDrawMessages()[drawnCard];
        //    else
        //    {
        //        if (deck.GetDefaultDrawMessages().Any())
        //           drawWithMessage.WithMessage=deck.GetDefaultDrawMessages().OrderBy(x => Random.value).First().Value;
        //    }

        //    return drawWithMessage;
        //}


        public Token Deal(string fromDeckSpecId)
        {
            var deckSpec = Watchman.Get<Compendium>().GetEntityById<DeckSpec>(fromDeckSpecId);
            var _drawPile = _dealersTable.GetDrawPile(fromDeckSpecId);
            var _forbiddenPile = _dealersTable.GetForbiddenPile(fromDeckSpecId);
            

            if (_drawPile.GetTotalStacksCount() == 0)
                Shuffle(fromDeckSpecId);
            
            //shuffling the might still not have given us a card - eg if all cards were forbidden.
            //so let's test again
            if (_drawPile.GetTotalStacksCount() > 0)
            {
                var cardDrawn = _drawPile.GetElementTokens().First();
                if(!deckSpec.ResetOnExhaustion)
                    ForbidRedrawOfCard(cardDrawn.Payload.EntityId,deckSpec);
                //This means that a 'don't reset on exhaustion' deck will forbid every card we draw, so a second shuffle won't renew anything.

                return cardDrawn;
            }

            else
                //if either the deck didn't reset on exhaustion,
                //or a reset has still left us with no cards,
                //then always return the default card
                return GetDefaultCardForDeck(deckSpec, _drawPile);
          
        }




        public void Shuffle(string deckSpecId)
        {
            var deckSpec = Watchman.Get<Compendium>().GetEntityById<DeckSpec>(deckSpecId);

            var drawPile = _dealersTable.GetDrawPile(deckSpecId);
            var forbiddenPile = _dealersTable.GetForbiddenPile(deckSpecId);
            var r = new System.Random();

            var forbiddenCards = forbiddenPile.GetElementTokens().Select(f => f.PayloadEntityId);
            var permittedCards = deckSpec.Spec.Except(forbiddenCards);

            foreach (var card in permittedCards.OrderBy(x=>r.Next()))
            {
                var t = new TokenCreationCommand().WithElementStack(card, 1);
                    t.Execute(Context.Unknown(), drawPile);
            }
        }

        /// <summary>
        /// This card is unique and has been drawn elsewhere, or belongs to the same uniqueness group as one that has been drawn elsewhere
        /// </summary>
        /// <param name="elementId"></param>
        public void IndicateUniqueElementManifested(string elementId)
        {
            foreach (var d in _dealersTable.GetDrawPiles())
            { 
                d.RetireTokensWhere(x => x.Payload.EntityId == elementId);

                //I do in fact need to do both of these... what happens if Ezeem has been drawn in the past, and then is shuffled back in later?
                //more elegant alternative might be to add the cards to the forbidden pile, and then autoremove all instances of that card from the matching draw pile whenever a card enters the forbidden pile

                var deckSpec = Watchman.Get<Compendium>().GetEntityById<DeckSpec>(d.GoverningSphereSpec.ActionId);

                if (deckSpec.Spec.Contains(elementId))
                    ForbidRedrawOfCard(elementId, deckSpec);
                
            }

        }

        public void IndicateElementInUniquenessGroupManifested(string elementUniquenessGroup)
        {
            var elementsToEliminate = Watchman.Get<Compendium>().GetEntitiesAsList<Element>()
                .Where(e => e.UniquenessGroup == elementUniquenessGroup);

            foreach (var element in elementsToEliminate)
            {
                IndicateUniqueElementManifested(element.Id);
            }
        }


        private void ForbidRedrawOfCard(string elementId, DeckSpec deckSpec)
        {
            var _forbiddenPile = _dealersTable.GetForbiddenPile(deckSpec.Id);
            if(_forbiddenPile.GetElementTokens().All(t => t.Payload.EntityId != elementId)) //don't clutter the forbidden pile unless there's no such card in there already
            {
                var t = new TokenCreationCommand().WithElementStack(elementId, 1);
                t.Execute(Context.Unknown(), _forbiddenPile);
            }
        }

        private Token GetDefaultCardForDeck(DeckSpec deckSpec, Sphere _drawPile)
        {
            var defaultCardCommand = new TokenCreationCommand().WithElementStack(deckSpec.DefaultCard, 1);
            var defaultCard = defaultCardCommand.Execute(Context.Unknown(), _drawPile);
            return defaultCard;
        }

    }
}
