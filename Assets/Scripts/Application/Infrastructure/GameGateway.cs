using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Abstract;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
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

                LoadGame(Watchman.Get<StageHand>().GamePersistenceProvider);
                ProvisionDropzoneToken();
            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }
        }


        public void LoadGame(GamePersistenceProvider gamePersistenceProviderSource)
        {
            //TODO: if we use the freshgameprovider, save the restart game json
            

            Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
            { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Paused, WithSFX = false });
            Watchman.Get<LocalNexus>().UILookAtMeEvent.Invoke(typeof(SpeedControlUI));
            try
            {

                gamePersistenceProviderSource.DepersistGameState(); //In the case of a Petromneme, this doesn't just deserialise, it will do the actual loading
                gamePersistenceProviderSource.ImportPetromnemeStateAfterTheAncientFashion();

                var gameState = gamePersistenceProviderSource.RetrievePersistedGameState();

                foreach (var c in gameState.CharacterCreationCommands)
                    c.Execute(Watchman.Get<Stable>());

                foreach (var t in gameState.TokenCreationCommands) //in the case of a petromneme, there aren't any tccs
                    t.Execute(new Context(Context.ActionSource.Loading));

                foreach (var n in gameState.NotificationCommands)
                {
                    Watchman.Get<Concursum>().ShowNotification(new NotificationArgs(n.Label, n.Description)); //ultimately, I'd like the float note to be a token, too - we're using AddCommand here currently just as a holder for the strings
                }

                
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

        private void ProvisionDropzoneToken()
        {
            var dropzoneLocation = new TokenLocation(Vector3.zero, Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());
            
            var dropzoneCreationCommand = new TokenCreationCommand(new DropzoneCreationCommand(), dropzoneLocation);
            dropzoneCreationCommand.Execute(new Context(Context.ActionSource.Unknown));
        }

        public async void EndGame(Ending ending, Token _anchor)
        {

            var chronicler = Watchman.Get<Chronicler>();

            chronicler.ChronicleGameEnd(Watchman.Get<SituationsCatalogue>().GetRegisteredSituations(), Watchman.Get<SphereCatalogue>().GetSpheres(), ending);
            var characterCreationCommand=CharacterCreationCommand.Reincarnate(Watchman.Get<Stable>().Protag().InProgressHistoryRecords, NullLegacy.Create(), ending);
            characterCreationCommand.Execute(Watchman.Get<Stable>());

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
