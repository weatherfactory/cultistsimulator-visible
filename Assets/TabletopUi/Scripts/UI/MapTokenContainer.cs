using Assets.Core.Interfaces;
using System.Collections;
using System.Collections.Generic;
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine;

namespace Assets.CS.TabletopUI {
    public class MapTokenContainer : TabletopTokenContainer {

        DoorSlot[] allSlots;
        DoorSlot activeSlot;

        public override void Initialise() {
            _elementStacksManager = new ElementStacksManager(this, "map");
            _elementStacksManager.EnforceUniqueStacks = true; // Martin: This ensures that this stackManager kills other copies when a unique is dropped in 

            choreo = Registry.Retrieve<Choreographer>();

            allSlots = GetComponentsInChildren<DoorSlot>();

            for (int i = 0; i < allSlots.Length; i++) 
                allSlots[i].Initialise();
        }

        public void SetActiveDoor(PortalEffect effect) {
            if (effect == PortalEffect.None) {
                activeSlot = null;
                return;
            }

            for (int i = 0; i < allSlots.Length; i++) {
                if (allSlots[i].portalType == effect) {
                    activeSlot = allSlots[i];
                    return;
                }
            }

            Debug.LogWarning("No Door Slot for " + effect + " found. Setting null");
            activeSlot = null;
        }

        public DoorSlot GetActiveDoor() {
            if (activeSlot == null) {
                Debug.LogWarning("We don't have an active door slot, using a random one");
                return allSlots[Random.Range(0, allSlots.Length)];
            }

            return activeSlot;
        }

        public override void Show(bool show) {
            if (show) {
                canvasGroupFader.Show();

                activeSlot = GetComponentInChildren<DoorSlot>();
                activeSlot.ShowGlow(false, false); // ensure we're not glowing
                return;
            }

            canvasGroupFader.Hide();

            if (activeSlot != null) {
                activeSlot = null;
            }
        }

        public void ShowDestinationsForStack(IElementStack stack, bool show) {
            if (activeSlot != null)
                activeSlot.ShowGlow(show, show);
        }
    }
}