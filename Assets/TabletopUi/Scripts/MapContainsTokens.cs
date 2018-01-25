using Assets.Core.Interfaces;
using System.Collections;
using System.Collections.Generic;
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine;

namespace Assets.CS.TabletopUI {
    public class MapContainsTokens : TabletopTokenContainer {

        [SerializeField] CanvasGroupFader canvasGroupFader;

        DoorSlot activeSlot;

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

        public void ShowDestinationsForStack(IElementStack stack) {
            if (activeSlot != null)
                activeSlot.ShowGlow(stack != null, stack != null);
        }
    }
}