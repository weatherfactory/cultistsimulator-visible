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
using Assets.TabletopUi.Scripts.Interfaces;
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
            float sTokenHorizSpace = (Registry.Get<PrefabFactory>().GetPrefab<VerbAnchor>().transform as RectTransform).rect.width + 20f;
            float sTokenVertiSpace = (Registry.Get<PrefabFactory>().GetPrefab<VerbAnchor>().transform as RectTransform).rect.height + 50f;

            // build verbs
            IVerb v = Registry.Get<ICompendium>().GetEntityById<BasicVerb>(legacy.StartingVerbId);
               
 
                SituationCreationCommand command = new SituationCreationCommand(v, null, SituationState.Unstarted);
               var controller=CreateSituation(command);
            controller.situationAnchor.transform.localPosition = new Vector3(-700f + sTokenHorizSpace, 200f -(0*sTokenVertiSpace));
                //this is left over from when we sometimes started with multiple verbs in a new legacy; those days might come again ofc, so I'm leaving the formula in

        }

        public SituationController CreateSituation(SituationCreationCommand command)
        {
            Core.Entities.Situation situation = new Core.Entities.Situation(command);
            var newToken = CreateAnchor(command);
            var newWindow = CreateSituationWindow(newToken);
            var situationController = new SituationController(Registry.Get<ICompendium>(), Registry.Get<Character>());
            newToken.Initialise(situation.Verb, situationController);
            newWindow.Initialise(situation.Verb, situationController);
            situationController.Initialise(situation, newToken, newWindow);

            return situationController;
        }

        public ISituationAnchor CreateAnchor(SituationCreationCommand situationCreationCommand)
        {

            var newAnchor = Registry.Get<PrefabFactory>().CreateSituationAnchorForVerb(situationCreationCommand.Verb,tableLevel);

            
              newAnchor.SetTokenContainer(tableLevel.GetComponent<AbstractTokenContainer>(), new Context(Context.ActionSource.Unknown));


            if (situationCreationCommand.LocationInfo != null)
                newAnchor.SaveLocationInfo = situationCreationCommand.LocationInfo;


            newAnchor.SetParticleSimulationSpace(tableLevel);
            return newAnchor;
        }

   
        public SituationWindow CreateSituationWindow(ISituationAnchor  anchor) {
            var situationWindow = Registry.Get<PrefabFactory>().CreateLocally<SituationWindow>(windowLevel);
            situationWindow.gameObject.SetActive(false);
            situationWindow.positioner.Initialise(anchor);
            return situationWindow;
        }

    }
}
