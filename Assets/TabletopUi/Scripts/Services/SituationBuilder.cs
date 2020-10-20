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
using Assets.Core.Enums;
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

        public Situation CreateSituation(SituationCreationCommand command)
        {
            Situation situation = new Situation(command);
            Registry.Get<SituationsCatalogue>().RegisterSituation(situation);


            var newAnchor = CreateAnchor(command);
            newAnchor.Initialise(situation);
            situation.AddSubscriber(newAnchor);

            var newWindow = CreateSituationWindow(newAnchor);
            newWindow.Initialise(situation);
            situation.AddSubscriber(newWindow);

            situation.AddContainers(newWindow.GetStartingSlots());
            situation.AddContainers(newWindow.GetOngoingSlots());
            situation.AddContainer(newWindow.GetStorageContainer());
            situation.AddContainer(newWindow.GetResultsContainer());
            


            if (command.SourceToken != null)
                SoundManager.PlaySfx("SituationTokenCreate");

            situation.ExecuteHeartbeat(0f);

            return situation;
        }


        public ISituationAnchor CreateAnchor(SituationCreationCommand situationCreationCommand)
        {

            var newAnchor = Registry.Get<PrefabFactory>().CreateSituationAnchorForVerb(situationCreationCommand.Verb,tableLevel);
            newAnchor.SetTokenContainer(tableLevel.GetComponent<AbstractTokenContainer>(), new Context(Context.ActionSource.Unknown));

            if (situationCreationCommand.LocationInfo != null)
                newAnchor.SaveLocationInfo = situationCreationCommand.LocationInfo;


          
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
