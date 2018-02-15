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

        public void CreateInitialTokensOnTabletop() {
            float sTokenHorizSpace = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.width + 20f;
            float sTokenVertiSpace = (PrefabFactory.GetPrefab<SituationToken>().transform as RectTransform).rect.height + 50f;

            // build verbs
            var verbs = Registry.Retrieve<ICompendium>().GetAllVerbs().Where(v => v.AtStart).ToList();

            for (int i = 0; i < verbs.Count; i++) {
                IVerb v = verbs[i];
                SituationCreationCommand command = new SituationCreationCommand(v, null, SituationState.Unstarted);
                var situationToken = CreateTokenWithAttachedControllerAndSituation(command);

                situationToken.transform.localPosition = new Vector3(-700f + sTokenHorizSpace, 200f - i * sTokenVertiSpace);
                //replace with locatorinfo
            }
        }

        public SituationToken CreateTokenWithAttachedControllerAndSituation(SituationCreationCommand situationCreationCommand, string locatorInfo = null) {
            var situationController = new SituationController(Registry.Retrieve<ICompendium>(), Registry.Retrieve<Character>());
            var newToken = PrefabFactory.CreateToken<SituationToken>(tableLevel, locatorInfo);

            var window = BuiltSituationWindow(newToken);

            situationController.Initialise(situationCreationCommand, newToken, window);

            return newToken;
        }



        private SituationWindow BuiltSituationWindow(SituationToken situationToken) {
            var situationWindow = PrefabFactory.CreateLocally<SituationWindow>(windowLevel);
            situationWindow.gameObject.SetActive(false);
            situationWindow.positioner.Initialise(situationToken.transform);

            return situationWindow;
        }

    }
}
