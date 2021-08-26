using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Entities.Verbs;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Infrastructure;
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.Services;
using SecretHistories.Spheres;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.Constants
{
    public class GameGateway:MonoBehaviour
    {


        public bool DontLoadGame;
        public void Awake()
        {
            var r = new Watchman();
            r.Register(this);
        }
        public void Start()
        {
            try
            {
                if (!DontLoadGame)
                    LoadGame(Watchman.Get<StageHand>().GamePersistenceProvider);

            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }
        }


        public async void LoadGame(GamePersistenceProvider gamePersistenceProviderSource)
        {
            Watchman.Get<Heart>().Metapause();

            try
            {
                gamePersistenceProviderSource.DepersistGameState();

                var gameState = gamePersistenceProviderSource.RetrievePersistedGameState();

                foreach (var c in gameState.CharacterCreationCommands)
                    c.Execute(Watchman.Get<Stable>());

                gameState.RootPopulationCommand.Execute(new Context(Context.ActionSource.Loading));

                
                foreach (var n in gameState.NotificationCommands)
                {
                    Watchman.Get<Concursum>().ShowNotification(new NotificationArgs(n.Notification.Title, n.Notification.Description)); //ultimately, I'd like the float note to be a token, too - we're using AddCommand here currently just as a holder for the strings
                }
            }
            catch (Exception e)
            {
                Watchman.Get<Concursum>().ShowNotification(
                    new NotificationArgs(Watchman.Get<ILocStringProvider>().Get("UI_LOADFAILEDTITLE"), Watchman.Get<ILocStringProvider>().Get("UI_LOADFAILEDDESC")));
           
                NoonUtility.LogException(e);
            }

  
            TimeSpan sinceCharacterCreated= DateTime.Now- Watchman.Get<Stable>().Protag().DateTimeCreated;
            //if the current protag has been in existence for less than one complete second, this is a fresh game. Save the current state, so they'll come straight here rather than via new game next time
            //and also save the restart state.
            if (sinceCharacterCreated.Seconds<=0)
            {
                await TryDefaultSave();
                await SaveRestartState();
            }

            Watchman.Get<Heart>().Unmetapause();

                Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
                { ControlPriorityLevel = 1, GameSpeed = gamePersistenceProviderSource.GetDefaultGameSpeed(), WithSFX = false });

        }

        private static async Task SaveRestartState()
        {
            var restartingGameStateProvider = new RestartingGameProvider();
       restartingGameStateProvider.Encaust(Watchman.Get<Stable>(), Watchman.Get<HornedAxe>());
            var saveTask = restartingGameStateProvider.SerialiseAndSaveAsync();
            var result = await saveTask;
        }


        public async Task<bool> TryDefaultSave()
        {
            if (!gameInSaveableState())
                return false;

            var game = new DefaultGamePersistenceProvider();
            game.Encaust(Watchman.Get<Stable>(), Watchman.Get<HornedAxe>());
            var saveTask = game.SerialiseAndSaveAsync();
            var result = await saveTask;
            return result;
        }

        private bool gameInSaveableState()
        {
            var numa = Watchman.Get<Numa>();
            if (numa == null)
                return true;

            return !numa.IsOtherworldActive();
        }

        

        public void EndGame(Ending ending, Token focusOnToken)
        {
            //chronicle ending
            var chronicler = Watchman.Get<Chronicler>();
            chronicler.ChronicleGameEnd(Watchman.Get<HornedAxe>().GetRegisteredSituations(), Watchman.Get<HornedAxe>().GetSpheres(), ending);

            //Mark character as defunct
            var characterCreationCommand=CharacterCreationCommand.Reincarnate(Watchman.Get<Stable>().Protag().InProgressHistoryRecords, NullLegacy.Create(), ending);
            characterCreationCommand.Execute(Watchman.Get<Stable>());

            
            //stop everything
            Watchman.Get<Heart>().Metapause();

            //stop current music clip
            Watchman.Get<BackgroundMusic>().FadeToSilence(3f);


            InstantiateCSEndingEffect(ending, focusOnToken);

            Watchman.Get<LocalNexus>().DisablePlayerInput(10f);

            Watchman.Get<LocalNexus>().AbortEvent.AddListener(FinalTransitionToEndingScreen);
            Watchman.Get<CamOperator>().PointCameraAtTableLevelVector2(focusOnToken.transform.position,2f, FinalTransitionToEndingScreen);
            //and zoom in?
            
            //Ending: music


        }



        public void FinalTransitionToEndingScreen()
        {
            StartCoroutine(FadeToEndingScreen());
        }

        

        private IEnumerator FadeToEndingScreen()
        {
            Watchman.Get<TabletopFadeOverlay>().FadeToBlack(2f);
            yield return new WaitForSeconds(2f);
            Watchman.Get<StageHand>().EndingScreen();
        }

        private GameObject InstantiateCSEndingEffect(Ending ending, Token focusOnToken)
        {
            var tokenTransform = focusOnToken.transform;
            string effectName;

            if (string.IsNullOrEmpty(ending.Anim))
                effectName = "DramaticLight";
            else
                effectName = ending.Anim;
            
            var prefab = Resources.Load("FX/EndGame/" + effectName);

            if (prefab == null)
                return null;

            var go = Instantiate(prefab, tokenTransform) as GameObject;
            go.transform.position = tokenTransform.position;
            go.transform.localScale = Vector3.one;
            go.SetActive(true);

            var effect = go.GetComponent<CardEffect>();

            if (effect != null)
                effect.StartAnim(tokenTransform);

            return go;
        }


        public async void LeaveGame()
        {
            Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.Paused, WithSFX = false });

          var saveResult=await TryDefaultSave();

            Watchman.Get<StageHand>().MenuScreen();

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
