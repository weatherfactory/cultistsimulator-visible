using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services {
    public class SituationBuilder {

        private Transform tableLevel;
        private Transform windowLevel;

        public SituationBuilder(Transform tableLevel, Transform windowLevel) {
            this.tableLevel = tableLevel;
            this.windowLevel = windowLevel;
        }

        public void CreateInitialTokensOnTabletop(Legacy legacy) {
            float sTokenHorizSpace = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.width + 20f;
            float sTokenVertiSpace = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.height + 50f;

            // build verbs
            IVerb v = Registry.Get<ICompendium>().GetEntityById<BasicVerb>(legacy.StartingVerbId);
               
 
                SituationCreationCommand command = new SituationCreationCommand(v, null, SituationState.Unstarted);
                var situationToken = CreateToken(command);

                situationToken.transform.localPosition = new Vector3(-700f + sTokenHorizSpace, 200f -(0*sTokenVertiSpace));
                //this is left over from when we sometimes started with multiple verbs in a new legacy; those days might come again ofc, so I'm leaving the formula in

        }

        public SituationToken CreateToken(SituationCreationCommand situationCreationCommand, string locatorInfo = null)
        {
            
            var newToken = PrefabFactory.CreateToken<SituationToken>(tableLevel, locatorInfo);
            newToken.SetParticleSimulationSpace(tableLevel);
            return newToken;
        }

        //public SituationToken _OLD(SituationCreationCommand situationCreationCommand, string locatorInfo = null) {
        //    var situationController = new SituationController(Registry.Get<ICompendium>(), Registry.Get<Character>());
        //    var newToken = PrefabFactory.CreateToken<SituationToken>(tableLevel, locatorInfo);
        //    newToken.SetParticleSimulationSpace(tableLevel);

        //    var window = BuiltSituationWindow(newToken);

        //    situationController.Initialise(situationCreationCommand, newToken, window);

        //    return newToken;
        //}



        public SituationWindow CreateSituationWindow(Transform  startingPosition) {
            var situationWindow = PrefabFactory.CreateLocally<SituationWindow>(windowLevel);
            situationWindow.gameObject.SetActive(false);
            situationWindow.positioner.Initialise(startingPosition);
            return situationWindow;
        }

    }
}
