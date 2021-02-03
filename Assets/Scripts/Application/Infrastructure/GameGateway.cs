using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Commands.SituationCommands;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Abstract;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Infrastructure;
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.Services;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.Constants
{
    public class GameGateway:MonoBehaviour
    {

        [SerializeField] private EndGameAnimController _endGameAnimController;

        public void Awake()
        {
            var r = new Watchman();
            r.Register(this);
        }
        public void Start()
        {

            try
            {
                if (!Watchman.Get<StageHand>().GamePersistence.Exists()) //we can roll BeginNewGame into the deserialisation call, actually
                {
                    Watchman.Get<GameGateway>().BeginNewGame();
                }
                else
                {
                    LoadGame(Watchman.Get<StageHand>().GamePersistence);
                }

                ProvisionDropzoneToken();
            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }
        }


        public void LoadGame(GamePersistence gamePersistenceSource)
        {
          

            Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
            { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Paused, WithSFX = false });
            Watchman.Get<LocalNexus>().UILookAtMeEvent.Invoke(typeof(SpeedControlUI));
            try
            {

                gamePersistenceSource.DeserialiseFromPersistence(); //In the case of a Petromneme, this doesn't just deserialise, it will do the actual loading
                gamePersistenceSource.ImportPetromnemeStateAfterTheAncientFashion();

                var gameState = gamePersistenceSource.RetrieveGameState();

                foreach (var t in gameState.TokenCreationCommands) //in the case of a petromneme, there aren't any tccs
                    t.Execute(new Context(Context.ActionSource.Loading));

                //in both old and new cases, the character has already been deserialised and loaded in Glory by now - but what about reloads?

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

        }

        private void ProvisionStartingVerb(Legacy activeLegacy, Sphere inSphere)
        {
            
            SituationCreationCommand startingSituation = new SituationCreationCommand(activeLegacy.StartingVerbId, NullRecipe.Create().Id, StateEnum.Unstarted);
            TokenCreationCommand startingToken=new TokenCreationCommand(startingSituation,TokenLocation.Default());

            startingToken.Execute(new Context(Context.ActionSource.Unknown));
            

        }

        private void ProvisionStartingElements(Legacy activeLegacy,Sphere inSphere)
        {
            AspectsDictionary startingElements = new AspectsDictionary();
            startingElements.CombineAspects(activeLegacy.Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.

            foreach (var e in startingElements)
            {
                var context = new Context(Context.ActionSource.Loading);

                var elementStackCreationCommand=new ElementStackCreationCommand(e.Key, e.Value);

                Token token = inSphere.ProvisionElementStackToken(elementStackCreationCommand,context);
                inSphere.Choreographer.PlaceTokenAtFreeLocalPosition(token, context);
            }
        }


        public void BeginNewGame()
        {
            Character character = Watchman.Get<Stable>().Protag();
            Sphere tabletopSphere = Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere();


            ProvisionStartingVerb(character.ActiveLegacy, tabletopSphere);
            
            ProvisionStartingElements(character.ActiveLegacy, tabletopSphere);

            Watchman.Get<Concursum>().ShowNotification(new NotificationArgs(character.ActiveLegacy.Label, character.ActiveLegacy.StartDescription));

            character.Reincarnate(character.ActiveLegacy, NullEnding.Create());
            Watchman.Get<Compendium>().SupplyLevers(character);
            Watchman.Get<StageHand>().ClearRestartingGameFlag();
        }

        private void ProvisionDropzoneToken()
        {
            var dropzoneLocation = new TokenLocation(Vector3.zero, Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());
            
            var dropzoneCreationCommand = new TokenCreationCommand(new DropzoneCreationCommand(), dropzoneLocation);
            dropzoneCreationCommand.Execute(new Context(Context.ActionSource.Unknown));
        }

        public async void EndGame(Ending ending, Token _anchor)
        {

            var character = Watchman.Get<Stable>().Protag();
            var chronicler = Watchman.Get<Chronicler>();

            chronicler.ChronicleGameEnd(Watchman.Get<SituationsCatalogue>().GetRegisteredSituations(), Watchman.Get<SphereCatalogue>().GetSpheres(), ending);
            character.Reincarnate(NullLegacy.Create(), ending);

            throw new NotImplementedException("inactive save here?");
            
            _endGameAnimController.TriggerEnd(_anchor, ending);
        }


        public async void LeaveGame()
        {
            Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.Paused, WithSFX = false });


            //ITableSaveState tableSaveState = new TableSaveState(Watchman.Get<SphereCatalogue>().GetSpheresOfCategory(SphereCategory.World).SelectMany(sphere => sphere.GetAllTokens())

            //    , Watchman.Get<SituationsCatalogue>().GetRegisteredSituations(), Watchman.Get<MetaInfo>());

            //var saveTask = Watchman.Get<GameSaveManager>()
            //    .SaveActiveGameAsync(tableSaveState, Watchman.Get<Stable>().Protag(), SourceForGameState.DefaultSave);

            //var success = await saveTask;


            //if (success)
            //{
            //    Watchman.Get<StageHand>().MenuScreen();
            //}
            //else
            //{
            //    // Save failed, need to let player know there's an issue
            //    // Autosave would wait and retry in a few seconds, but player is expecting results NOW.
            //    Watchman.Get<LocalNexus>().ToggleOptionsEvent.Invoke();
            //    GameSaveManager.ShowSaveError();
            //}
        }
    }
}
