using Assets.Core.Interfaces;
using System.Collections;
using System.Collections.Generic;
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine;

namespace Assets.CS.TabletopUI {
    public class MapContainsTokens : TabletopTokenContainer {

        [SerializeField] CanvasGroupFader canvasGroupFader;

        DoorSlot activeSlot;

        public override void Initialise() {
            _elementStacksManager = new ElementStacksManager(this, "map");
            _elementStacksManager.EnforceUniqueStacks = true; // Martin: This ensures that this stackManager kills other copies when a unique is dropped in 

            choreo = Registry.Retrieve<Choreographer>();
        }

        public DoorSlot GetDoor() {
            if (activeSlot == null)
                activeSlot = GetComponentInChildren<DoorSlot>();

            return activeSlot;
        }

        public void Show(bool show) {
            if (show) {
                canvasGroupFader.Show();

                activeSlot = GetComponentInChildren<DoorSlot>();
                activeSlot.onCardDropped += HandleOnSlotFilled;
                activeSlot.ShowGlow(false, false); // ensure we're not glowing
                return;
            }

            canvasGroupFader.Hide();

            if (activeSlot != null) {
                activeSlot.onCardDropped -= HandleOnSlotFilled;
                activeSlot = null;
            }
        }

        void HandleOnSlotFilled(IElementStack stack) {
            // Close map, retrieve the card
            Registry.Retrieve<MapController>().HideMansusMap(activeSlot.transform, stack);
        }

        public void ShowDestinationsForStack(IElementStack stack, bool show) {
            if (activeSlot != null)
                activeSlot.ShowGlow(show, show);
        }
    }
}