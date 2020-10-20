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

            situation.AddContainer(ContainerCategory.Starting,newWindow.);
            situation.AddContainer(ContainerCategory.SituationStorage, newWindow.GetStorageContainer());
            


            if (command.SourceToken != null)
                SoundManager.PlaySfx("SituationTokenCreate");

            situation.ExecuteHeartbeat(0f);

            return situation;
        }

        void InitialiseForActiveSituation()
        {
            if (Situation.State == SituationState.FreshlyStarted)
                Situation.Start();

            //ugly subclause here. SituationClock.Start largely duplicates the constructor. I'm trying to use the same code path for recreating situations from a save file as for beginning a new situation
            //possibly just separating out FreshlyStarted would solve it

            situationWindowAsStorage.SetOngoing(Situation.currentPrimaryRecipe);

            situationAnchor.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining, CurrentEndingFlavourToSignal);
            situationWindowAsView.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining, CurrentEndingFlavourToSignal);

            //this is a little ugly here; but it makes the intent clear. The best way to deal with it is probably to pass the whole Command down to the situationwindow for processing.
            if (Situation.OverrideTitle != null)
                situationWindowAsView.Title = Situation.OverrideTitle;

            UpdateSituationDisplayForPossiblePredictedRecipe();

            if (Situation.currentPrimaryRecipe != null && Situation.currentPrimaryRecipe.BurnImage != null)
                BurnImageUnderToken(Situation.currentPrimaryRecipe.BurnImage);

        }

        void InitialiseCompletedSituation()
        {
            situationWindowAsStorage.SetComplete();

            //this is a little ugly here; but it makes the intent clear. The best way to deal with it is probably to pass the whole Command down to the situationwindow for processing.
            if (Situation.OverrideTitle != null)
                situationWindowAsView.Title = Situation.OverrideTitle;

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
