using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    public class GameGateway:MonoBehaviour
    {
        public void Awake()
        {
            var r = new Registry();
            r.Register(this);
        }
        public void Start()
        {
            try
            {

                if (Registry.Get<StageHand>().SourceForGameState == SourceForGameState.NewGame)
                {
                    Registry.Get<GameGateway>().BeginNewGame();
                }
                else
                {
                    LoadGame(Registry.Get<StageHand>().SourceForGameState);
                }
            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }
        }


        public void LoadGame(SourceForGameState gameStateSource)
        {
            Compendium compendium = Registry.Get<Compendium>();


            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
            { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Paused, WithSFX = false });
            Registry.Get<LocalNexus>().UILookAtMeEvent.Invoke(typeof(SpeedControlUI));
            try
            {

                Registry.Get<GameSaveManager>().LoadTabletopState(gameStateSource, 
                    Registry.Get<SphereCatalogue>().GetDefaultWorldSphere());


                var allSituationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();
                foreach (var s in allSituationControllers)
                {
                    if (s.IsOpen)
                    {
                        s.OpenAtCurrentLocation();
                    }
                }

                Registry.Get<Concursum>().ShowNotification(
                     new NotificationArgs(Registry.Get<ILocStringProvider>().Get("UI_LOADEDTITLE"), Registry.Get<ILocStringProvider>().Get("UI_LOADEDDESC")));
      
            }
            catch (Exception e)
            {
                Registry.Get<Concursum>().ShowNotification(
                    new NotificationArgs(Registry.Get<ILocStringProvider>().Get("UI_LOADFAILEDTITLE"), Registry.Get<ILocStringProvider>().Get("UI_LOADFAILEDDESC")));
           
                NoonUtility.LogException(e);
            }

            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
            { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Paused, WithSFX = false });

            Registry.Get<LocalNexus>().UILookAtMeEvent.Invoke(typeof(SpeedControlUI));

            var activeLegacy = Registry.Get<Character>().ActiveLegacy;



        }

        private void ProvisionStartingVerb(Legacy activeLegacy, Sphere inSphere)
        {
            IVerb v = Registry.Get<Compendium>().GetEntityById<BasicVerb>(activeLegacy.StartingVerbId);

            SituationCreationCommand command = new SituationCreationCommand(v, NullRecipe.Create(), StateEnum.Unstarted,
                new TokenLocation(0f, 0f, -100f, inSphere.GetPath()));

            var situation = Registry.Get<SituationBuilder>().CreateSituationWithAnchorAndWindow(command);

            situation.ExecuteHeartbeat(0f);

        }

        private void ProvisionStartingElements(Legacy activeLegacy,Sphere inSphere)
        {
            AspectsDictionary startingElements = new AspectsDictionary();
            startingElements.CombineAspects(activeLegacy.Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.

            foreach (var e in startingElements)
            {
                var context = new Context(Context.ActionSource.Loading);

                Token token = inSphere.ProvisionElementStackToken(e.Key, e.Value, Source.Existing(), context, Element.EmptyMutationsDictionary());
                inSphere.Choreographer.PlaceTokenAtFreeLocalPosition(token, context);
            }
        }


        public void BeginNewGame()
        {
            Character character = Registry.Get<Character>();
            Sphere tabletopSphere = Registry.Get<SphereCatalogue>().GetDefaultWorldSphere();


            ProvisionStartingVerb(character.ActiveLegacy, tabletopSphere);
            ProvisionDropzoneSituation();
            
            ProvisionStartingElements(character.ActiveLegacy, tabletopSphere);

            Registry.Get<Concursum>().ShowNotification(new NotificationArgs(character.ActiveLegacy.Label, character.ActiveLegacy.StartDescription));

            character.Reset(character.ActiveLegacy, null);
            Registry.Get<Compendium>().SupplyLevers(character);
            Registry.Get<StageHand>().ClearRestartingGameFlag();
        }

        private Situation ProvisionDropzoneSituation()
        {
            //if not, create it
            var dropzoneRecipe = Registry.Get<Compendium>().GetEntityById<Recipe>(NoonConstants.DROPZONE_RECIPE_ID);
            var dropzoneVerb = Registry.Get<Compendium>().GetVerbForRecipe(dropzoneRecipe);
            var dropzoneLocation = new TokenLocation(Vector3.zero, Registry.Get<SphereCatalogue>().GetDefaultWorldSphere());

            var dropzoneSituation = dropzoneVerb.CreateDefaultSituation(dropzoneLocation);
        

            return dropzoneSituation;
        }


        public async void LeaveGame()
        {
            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.Paused, WithSFX = false });


            ITableSaveState tableSaveState = new TableSaveState(Registry.Get<SphereCatalogue>().GetSpheresOfCategory(SphereCategory.World).SelectMany(sphere => sphere.GetAllTokens())

                , Registry.Get<SituationsCatalogue>().GetRegisteredSituations(), Registry.Get<MetaInfo>());

            var saveTask = Registry.Get<GameSaveManager>()
                .SaveActiveGameAsync(tableSaveState, Registry.Get<Character>(), SourceForGameState.DefaultSave);

            var success = await saveTask;


            if (success)
            {
                Registry.Get<StageHand>().MenuScreen();
            }
            else
            {
                // Save failed, need to let player know there's an issue
                // Autosave would wait and retry in a few seconds, but player is expecting results NOW.
                Registry.Get<LocalNexus>().ToggleOptionsEvent.Invoke();
                GameSaveManager.ShowSaveError();
            }
        }
    }
}
