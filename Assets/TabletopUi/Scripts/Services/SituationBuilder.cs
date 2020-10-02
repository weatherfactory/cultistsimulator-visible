using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Infrastructure;
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
            float sTokenHorizSpace = (Registry.Get<PrefabFactory>().GetPrefab<SituationToken>().transform as RectTransform).rect.width + 20f;
            float sTokenVertiSpace = (Registry.Get<PrefabFactory>().GetPrefab<SituationToken>().transform as RectTransform).rect.height + 50f;

            // build verbs
            IVerb v = Registry.Get<ICompendium>().GetEntityById<BasicVerb>(legacy.StartingVerbId);
               
 
                SituationCreationCommand command = new SituationCreationCommand(v, null, SituationState.Unstarted);
               var controller=CreateSituation(command);
            controller.situationToken.transform.localPosition = new Vector3(-700f + sTokenHorizSpace, 200f -(0*sTokenVertiSpace));
                //this is left over from when we sometimes started with multiple verbs in a new legacy; those days might come again ofc, so I'm leaving the formula in

        }

        public SituationController CreateSituation(SituationCreationCommand command)
        {
            Situation situation = new Situation(command);
            var newToken = Registry.Get<SituationBuilder>().CreateToken(command);
            var newWindow = Registry.Get<SituationBuilder>().CreateSituationWindow(newToken.transform);
            var situationController = new SituationController(Registry.Get<ICompendium>(), Registry.Get<Character>());
            newToken.Initialise(situation.Verb, situationController);
            newWindow.Initialise(situation.Verb, situationController);
            situationController.Initialise(situation, newToken, newWindow);

            return situationController;
        }

        public SituationToken CreateToken(SituationCreationCommand situationCreationCommandl)
        {

            var newToken = Registry.Get<PrefabFactory>().CreateLocally<SituationToken>(tableLevel);


            newToken.SetTokenContainer(tableLevel.GetComponent<AbstractTokenContainer>(), new Context(Context.ActionSource.Unknown));


            if (situationCreationCommandl.LocationInfo != null)
                newToken.SaveLocationInfo = situationCreationCommandl.LocationInfo;


            newToken.SetParticleSimulationSpace(tableLevel);
            return newToken;
        }

   
        public SituationWindow CreateSituationWindow(Transform  startingPosition) {
            var situationWindow = Registry.Get<PrefabFactory>().CreateLocally<SituationWindow>(windowLevel);
            situationWindow.gameObject.SetActive(false);
            situationWindow.positioner.Initialise(startingPosition);
            return situationWindow;
        }

    }
}
