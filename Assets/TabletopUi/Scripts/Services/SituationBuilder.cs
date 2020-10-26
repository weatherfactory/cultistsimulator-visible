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

        }

        public Situation CreateSituation(SituationCreationCommand command)
        {
            Situation situation = new Situation(command);
            Registry.Get<SituationsCatalogue>().RegisterSituation(situation);

            var newAnchor = CreateAnchor(command);

            situation.AttachAnchor(newAnchor);
            
            var newWindow = CreateSituationWindow(newAnchor);
            situation.AttachWindow(newWindow);


            //if token has been spawned from an existing token, animate its appearance
            if (command.SourceToken != null)
            {
                newAnchor.AnimateTo(
                     1f,
                     command.SourceToken.RectTransform.anchoredPosition3D,
                     Registry.Get<Choreographer>().GetFreePosWithDebug(newAnchor, command.SourceToken.RectTransform.anchoredPosition, 3),
                     null,
                     0f,
                     1f);
            }
            else
            {
                Registry.Get<Choreographer>().ArrangeTokenOnTable(newAnchor, null);
            }



            if (command.SourceToken != null)
                SoundManager.PlaySfx("SituationTokenCreate");

            situation.ExecuteHeartbeat(0f);




            return situation;
        }


        public ISituationAnchor CreateAnchor(SituationCreationCommand situationCreationCommand)
        {

            var newAnchor = Registry.Get<PrefabFactory>().CreateSituationAnchorForVerb(situationCreationCommand.Verb,tableLevel);
            newAnchor.SetTokenContainer(tableLevel.GetComponent<TokenContainer>(), new Context(Context.ActionSource.Unknown));

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
