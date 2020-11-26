using System;
using Assets.Core.Interfaces;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.CS.TabletopUI {
    public class MapSphere : Sphere {

        DoorSlot[] allSlots;
        DoorSlot activeSlot;

        public override bool AllowStackMerge { get { return false; } }
        public override SphereCategory SphereCategory { get; }

        
        public void SetActiveDoor(PortalEffect effect) {
            if (effect == PortalEffect.None) {
                activeSlot = null;
                return;
            }

            activeSlot = null;

            for (int i = 0; i < allSlots.Length; i++) {
                if (allSlots[i].portalType == effect) {
                    allSlots[i].SetAsActive(true);
                    activeSlot = allSlots[i];
                    //Debug.Log("Setting Active Slot " + activeSlot + ".");
                }
                else {
                    allSlots[i].SetAsActive(false);
                }
            }

            if (activeSlot == null) { 
               // Debug.LogWarning("No Door Slot for " + effect + " found. Setting a random one");
                activeSlot = allSlots[Random.Range(0, allSlots.Length)];
                activeSlot.SetAsActive(true);
            }
        }

        public DoorSlot GetActiveDoor() {
            if (activeSlot == null) {
                throw new ApplicationException("No active door specified");
              // Debug.LogWarning("We don't have an active door slot, using a random one");
               // return allSlots[Random.Range(0, allSlots.Length)];
            }

            return activeSlot;
        }


    }
}