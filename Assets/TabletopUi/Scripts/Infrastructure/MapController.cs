using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;
using Assets.TabletopUi.Scripts.Services;
using Assets.Core.Entities;

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

            cards = new ElementStackToken[3];

            // Display face-up card next to door
            cards[0] = BuildCard(activeDoor.cardPositions[0].transform.position);
            cards[0].FlipToFaceUp(true);

            // Display face down card next to locations
            cards[1] = BuildCard(activeDoor.cardPositions[1].transform.position);
            cards[1].FlipToFaceDown(true);

            cards[2] = BuildCard(activeDoor.cardPositions[2].transform.position);
            cards[2].FlipToFaceDown(true);

            // When one face-down card is turned, remove all face up cards.
            // On droping on door: Return
        }

        ElementStackToken BuildCard(Vector3 position, string id = "funds") {
            var newCard = PrefabFactory.CreateToken<ElementStackToken>(transform.parent);
            newCard.Populate(id, 1, Source.Fresh());

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
                        cards[i].Retire(true);

            cards = null;
        }

        void HandleOnSlotFilled(IElementStack stack) {
            var activeDoor = _mapTokenContainer.GetActiveDoor();
            HideMansusMap(activeDoor.transform, stack);
        }

        public void CleanupMap() {
            var activeDoor = _mapTokenContainer.GetActiveDoor();
            activeDoor.onCardDropped -= HandleOnSlotFilled;
            _mapTokenContainer.SetActiveDoor(PortalEffect.None);
            cards = null;
        }

        // -- ANIMATION ------------------------

        public void ShowMansusMap(Transform effectCenter, bool show = true)
        {
            if (_mapAnimation.CanShow(show) == false)
                return;

            if (!show) // hide the container
                _mapTokenContainer.Show(false);

            // TODO: should probably lock interface? No zoom, no tabletop interaction. Check EndGameAnim for ideas

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
            // TODO: should probably unlock interface? No zoom, no tabletop interaction
        }

        public void HideMansusMap(Transform effectCenter, IElementStack stack)
        {
            Registry.Retrieve<TabletopManager>().HideMansusMap(effectCenter, (ElementStackToken)stack);
        }

#if DEBUG
        public void CloseMap() {
            Registry.Retrieve<TabletopManager>().HideMansusMap(_mapTokenContainer.GetActiveDoor().transform, null);
        }
#endif
    }
}
