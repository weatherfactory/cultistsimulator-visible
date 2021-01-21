using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Commands.SituationCommands;
using SecretHistories.Abstract;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Spheres.Angels;
using SecretHistories.Constants;
using SecretHistories.Entities.Verbs;
using SecretHistories.Infrastructure;
using SecretHistories.Services;

using UnityEngine;

namespace SecretHistories.Constants
{
    public class GameGateway:MonoBehaviour
    {
        public bool KeepBoardEmpty;
        public void Awake()
        {
            var r = new Watchman();
            r.Register(this);
        }
        public void Start()
        {
            if (KeepBoardEmpty)
                return;

            try
            {

                if (Watchman.Get<StageHand>().SourceForGameState == SourceForGameState.NewGame)
                {
                    Watchman.Get<GameGateway>().BeginNewGame();
                }
                else
                {
                    LoadGame(Watchman.Get<StageHand>().SourceForGameState);
                }
            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }
        }


        public void LoadGame(SourceForGameState gameStateSource)
        {
          

            Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
            { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Paused, WithSFX = false });
            Watchman.Get<LocalNexus>().UILookAtMeEvent.Invoke(typeof(SpeedControlUI));
            try
            {

                Watchman.Get<GameSaveManager>().LoadTabletopState(gameStateSource, 
                    Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());


                var allSituationControllers = Watchman.Get<SituationsCatalogue>().GetRegisteredSituations();
                foreach (var s in allSituationControllers)
                {
                    if (s.IsOpen)
                    {
                        s.OpenAtCurrentLocation();
                    }
                }

                Watchman.Get<Concursum>().ShowNotification(
                     new NotificationArgs(Watchman.Get<ILocStringProvider>().Get("UI_LOADEDTITLE"), Watchman.Get<ILocStringProvider>().Get("UI_LOADEDDESC")));
      
            }
            catch (Exception e)
            {
                Watchman.Get<Concursum>().ShowNotification(
                    new NotificationArgs(Watchman.Get<ILocStringProvider>().Get("UI_LOADFAILEDTITLE"), Watchman.Get<ILocStringProvider>().Get("UI_LOADFAILEDDESC")));
           
                NoonUtility.LogException(e);
            }

            Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
            { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Paused, WithSFX = false });

            Watchman.Get<LocalNexus>().UILookAtMeEvent.Invoke(typeof(SpeedControlUI));

            ProvisionDropzoneToken();



        }

        private void ProvisionStartingVerb(Legacy activeLegacy, Sphere inSphere)
        {
            IVerb v = Watchman.Get<Compendium>().GetEntityById<BasicVerb>(activeLegacy.StartingVerbId);

            SituationCreationCommand command = new SituationCreationCommand(v, NullRecipe.Create(), StateEnum.Unstarted,
                new TokenLocation(0f, 0f, -100f, inSphere.GetPath())).WithDefaultAttachments();

            var situationCat = Watchman.Get<SituationsCatalogue>();
            var situation = command.Execute(situationCat);


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
            Character character = Watchman.Get<Character>();
            Sphere tabletopSphere = Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere();


            ProvisionStartingVerb(character.ActiveLegacy, tabletopSphere);
            ProvisionDropzoneToken();
            
            ProvisionStartingElements(character.ActiveLegacy, tabletopSphere);

            Watchman.Get<Concursum>().ShowNotification(new NotificationArgs(character.ActiveLegacy.Label, character.ActiveLegacy.StartDescription));

            character.Reset(character.ActiveLegacy, null);
            Watchman.Get<Compendium>().SupplyLevers(character);
            Watchman.Get<StageHand>().ClearRestartingGameFlag();
        }

        private void ProvisionDropzoneToken()
        {
         
            
            var dropzoneVerb = new DropzoneVerb();
            var dropzoneLocation = new TokenLocation(Vector3.zero, Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());
            var dropzoneCreationCommand = new TokenCreationCommand(dropzoneVerb, dropzoneLocation, null);
            dropzoneCreationCommand.Execute(Watchman.Get<SphereCatalogue>());
        }


        public async void LeaveGame()
        {
            Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.Paused, WithSFX = false });


            ITableSaveState tableSaveState = new TableSaveState(Watchman.Get<SphereCatalogue>().GetSpheresOfCategory(SphereCategory.World).SelectMany(sphere => sphere.GetAllTokens())

                , Watchman.Get<SituationsCatalogue>().GetRegisteredSituations(), Watchman.Get<MetaInfo>());

            var saveTask = Watchman.Get<GameSaveManager>()
                .SaveActiveGameAsync(tableSaveState, Watchman.Get<Character>(), SourceForGameState.DefaultSave);

            var success = await saveTask;


            if (success)
            {
                Watchman.Get<StageHand>().MenuScreen();
            }
            else
            {
                // Save failed, need to let player know there's an issue
                // Autosave would wait and retry in a few seconds, but player is expecting results NOW.
                Watchman.Get<LocalNexus>().ToggleOptionsEvent.Invoke();
                GameSaveManager.ShowSaveError();
            }
        }
    }
}
