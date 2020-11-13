using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Logic;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
#pragma warning disable 0649
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.SlotsContainers;
using Noon;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI {

    public class VerbAnchor : Token
    {
        
        public override bool Retire() {
            if (!Defunct)
                SpawnKillFX();

            return base.Retire();
        }

        void SpawnKillFX() {
            var prefab = Resources.Load("FX/SituationToken/SituationTokenVanish");

            if (prefab == null)
                return;

            var go = Instantiate(prefab, transform.parent) as GameObject;
            go.transform.position = transform.position;
            go.transform.localScale = Vector3.one;
            go.SetActive(true);
        }


        // None of this should do view changes here. We're deferring to the SitController or TokenContainer





        private void animDone(VerbAnchor token)
        {
            Sphere.DisplayHere(token, new Context(Context.ActionSource.AnimEnd));
        }

        public void DumpOutputStacks()
        {
            _situation.CollectOutputStacks();
        }

        private void SwapOutManifestation(IManifestation oldManifestation, IManifestation newManifestation, RetirementVFX vfxForOldManifestation)
        {
            var manifestationToRetire = oldManifestation;
            _manifestation = newManifestation;

            manifestationToRetire.Retire(vfxForOldManifestation, OnSwappedOutManifestationRetired);

        }

        private void OnSwappedOutManifestationRetired()
        {

        }

   
    }
}
