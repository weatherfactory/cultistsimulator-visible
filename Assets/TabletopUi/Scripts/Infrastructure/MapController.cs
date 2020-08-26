using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;
using Assets.TabletopUi.Scripts.Services;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Logic;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class MapController: MonoBehaviour
    {
        private MapTokenContainer _mapTokenContainer;
        //private TabletopBackground _mapBackground;
        private MapAnimation _mapAnimation;

        private ElementStackToken[] cards;

        public void Initialise(MapTokenContainer mapTokenContainer, TabletopBackground mapBackground, MapAnimation mapAnimation) {
            mapBackground.gameObject.SetActive(false);

            mapTokenContainer.gameObject.SetActive(false);
            _mapTokenContainer = mapTokenContainer;

            //_mapBackground = mapBackground;

            _mapAnimation = mapAnimation;
            mapAnimation.Init();
        }

        public void SetupMap(PortalEffect effect) {
            // Highlight active door
            _mapTokenContainer.SetActiveDoor(effect);
            var activeDoor = _mapTokenContainer.GetActiveDoor();
            activeDoor.onCardDropped += HandleOnSlotFilled;


            //get card position names
            //populate card positions 1,2,3 from decks with names of positions
            cards = new ElementStackToken[3];

            // Display face-up card next to door

            var character=Registry.Get<Character>();
            var dealer=new Dealer(character);

            string doorDeckId = activeDoor.GetDeckName(0);
            IDeckInstance doorDeck =character.GetDeckInstanceById(doorDeckId);
            if (doorDeck==null)
                throw new ApplicationException("MapController couldn't find a mansus deck for the specified door with ID " + doorDeckId);

            string subLocationDeck1Id = activeDoor.GetDeckName(1);
            IDeckInstance subLocationDeck1 = character.GetDeckInstanceById(subLocationDeck1Id);
            if (doorDeck == null)
                throw new ApplicationException("MapController couldn't find a mansus deck for location1 with ID " + subLocationDeck1Id);

            string subLocationDeck2Id = activeDoor.GetDeckName(2);
            IDeckInstance subLocationDeck2 = character.GetDeckInstanceById(subLocationDeck2Id);
            if (doorDeck == null)
                throw new ApplicationException("MapController couldn't find a mansus deck for location2 with ID " + subLocationDeck2Id);

			DrawWithMessage dwm0 = dealer.DealWithMessage(doorDeck);
            cards[0] = BuildCard(activeDoor.cardPositions[0].transform.position, dwm0.DrawnCard,activeDoor.portalType, dwm0.WithMessage);
            cards[0].FlipToFaceUp(true);

            // Display face down cards next to locations
			int counter = 0;
            DrawWithMessage dwm1 = dealer.DealWithMessage(subLocationDeck1);
			while (dwm1.DrawnCard == dwm0.DrawnCard)
			{
				dwm1 = dealer.DealWithMessage(subLocationDeck1);	// Repeat until we get a non-matching card
				counter++;
				Debug.Assert( counter<10, "SetupMap() : Unlikely number of retries. Could be stuck in while loop?" );
			}
            cards[1] = BuildCard(activeDoor.cardPositions[1].transform.position, dwm1.DrawnCard, activeDoor.portalType,  dwm1.WithMessage);
            cards[1].FlipToFaceDown(true);

			counter = 0;
            DrawWithMessage dwm2 = dealer.DealWithMessage(subLocationDeck2);
			while (dwm2.DrawnCard == dwm1.DrawnCard || dwm2.DrawnCard == dwm0.DrawnCard)
			{
				dwm2 = dealer.DealWithMessage(subLocationDeck2);	// Repeat until we get a non-matching card
				counter++;
				Debug.Assert( counter<10, "SetupMap() : Unlikely number of retries. Could be stuck in while loop?" );
			}
            cards[2] = BuildCard(activeDoor.cardPositions[2].transform.position, dwm2.DrawnCard, activeDoor.portalType, dwm2.WithMessage);
            cards[2].FlipToFaceDown(true);

            // When one face-down card is turned, remove all face up cards.
            // On droping on door: Return
        }


        ElementStackToken BuildCard(Vector3 position, string id,PortalEffect portalType,string mansusJournalEntryMessage) {
            var newCard = PrefabFactory.CreateToken<ElementStackToken>(transform.parent);
            newCard.Populate(id, 1, Source.Fresh());

            newCard.IlluminateLibrarian.AddMansusJournalEntry(mansusJournalEntryMessage);

            // Accepting stack will trigger overlap checks, so make sure we're not in the default pos but where we want to be.
            newCard.transform.position = position;

            var stacksManager = _mapTokenContainer.GetElementStacksManager();
            stacksManager.AcceptStack(newCard, new Context(Context.ActionSource.Loading));

            // Accepting stack may put it to pos Vector3.zero, so this is last
            newCard.transform.position = position;
            newCard.transform.localScale = Vector3.one;
            newCard.onTurnFaceUp += HandleOnCardTurned;
            return newCard;
        }

        void HandleOnCardTurned(ElementStackToken cardTurned) {
            if (cards != null)
                for (int i = 0; i < cards.Length; i++)
                    if (cards[i] != cardTurned)
                    {
                        cards[i].FlipToFaceUp();
                        cards[i].Retire(CardVFX.CardLightDramatic);
                    }

            cards = null;
        }

        void HandleOnSlotFilled(IElementStack stack) {
            var activeDoor = _mapTokenContainer.GetActiveDoor();
            HideMansusMap(activeDoor.transform, stack);
        }

        public void CleanupMap(ElementStackToken pickedStack) {
            var activeDoor = _mapTokenContainer.GetActiveDoor();
            activeDoor.onCardDropped -= HandleOnSlotFilled;
            _mapTokenContainer.SetActiveDoor(PortalEffect.None);

            // Kill all still existing cards
            if (cards != null) {
                foreach (var item in cards) {
                    if (item != pickedStack)
                        item.Retire(CardVFX.None);
                }
            }

            cards = null;
        }

        // -- ANIMATION ------------------------

        public void ShowMansusMap(Transform effectCenter, bool show = true)
        {
            if (_mapAnimation.CanShow(show) == false)
                return;

            if (!show) // hide the container
                _mapTokenContainer.Show(false);

            // Lock interface. No zoom, no tabletop interaction? Check EndGameAnim for ideas
			DraggableToken.draggingEnabled = false;

            _mapAnimation.onAnimDone += OnMansusMapAnimDone;
            _mapAnimation.SetCenterForEffect(effectCenter);
            _mapAnimation.Show(show); // starts coroutine that calls onManusMapAnimDone when done
            _mapAnimation.Show(show);
        }

        void OnMansusMapAnimDone(bool show)
        {
            _mapAnimation.onAnimDone -= OnMansusMapAnimDone;

            if (show) // show the container
                _mapTokenContainer.Show(true);

            // Unlock interface. No zoom, no tabletop interaction
			DraggableToken.draggingEnabled = true;
        }

        public void HideMansusMap(Transform effectCenter, IElementStack stack)
        {
            Registry.Get<TabletopManager>().ReturnFromMansus(effectCenter, (ElementStackToken)stack);
        }


//not currently in use, preserve for quick debug
        public void CloseMap() {
            Registry.Get<TabletopManager>().ReturnFromMansus(_mapTokenContainer.GetActiveDoor().transform, null);
        }
    }
}
