using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;
using SecretHistories.UI;

using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Logic
{
    public class Dealer
    {
        private DealersTable _dealersTable;

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
            

            if (_drawPile.GetTotalStacksCount() == 0)
            {
                //if the deck is exhausted:
                //--some decks reset, so we can have an infinite supply of whatevers.
                if (deckSpec.ResetOnExhaustion)
                    Shuffle(fromDeckSpecId);
            }
            //Conceivably, resetting the deck might still not have given us a card,
            //so let's test again
            if (_drawPile.GetTotalStacksCount() > 0)
            {
                var cardDrawn = _drawPile.GetElementTokens().First();
                return cardDrawn;
            }

            else
                //if either the deck didn't reset on exhaustion,
                //or a reset has still left us with no cards,
                //then always return the default card
            {
                var defaultCardCommand=new TokenCreationCommand().WithElementStack(deckSpec.DefaultCard,1);
                var defaultCard=defaultCardCommand.Execute(Context.Unknown(), _drawPile);
                return defaultCard;
            }
        }


        public void Shuffle(string deckSpecId)
        {
            var deckSpec = Watchman.Get<Compendium>().GetEntityById<DeckSpec>(deckSpecId);

            var drawPile = _dealersTable.GetDrawPile(deckSpecId);
            var forbiddenPile = _dealersTable.GetForbiddenPile(deckSpecId);
            
            foreach (var card in deckSpec.Spec)
            {
                if(!forbiddenPile.GetElementTokens().Exists(e=>e.Payload.EntityId==card))
                {
                    var t = new TokenCreationCommand().WithElementStack(card, 1);
                    t.Execute(Context.Unknown(), drawPile);
                }
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

                //I do need to do both of these... what happens if Ezeem has been drawn in the past, and then is shuffled back in later?
                //more elegant alternative might be to add the cards to the forbidden pile, and then autoremove all instances of that card from the matching draw pile whenever a card enters the forbidden pile

                var deckSpec = Watchman.Get<Compendium>().GetEntityById<DeckSpec>(d.GoverningSphereSpec.ActionId);

                if (deckSpec.Spec.Contains(elementId))
                {
                    var _forbiddenPile = _dealersTable.GetForbiddenPile(deckSpec.Id);

                    var t = new TokenCreationCommand().WithElementStack(elementId, 1);
                    t.Execute(Context.Unknown(), _forbiddenPile);
                }
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



    }
}
