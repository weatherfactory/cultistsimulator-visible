using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Constants;
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

namespace SecretHistories.Infrastructure
{
    public class GameGateway:MonoBehaviour
    {


        public bool DontLoadGame;
        private Coroutine fadeToEndingCoroutine;
        private bool DefaultSaveInProgress; //This locks for TryDefaultSave only. If we move to coroutines, we can check for the existence of a coroutine.
        
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

        //uses Stagehand to reload the scene as from the beginning
        public void LoadAfresh()
        {
           Watchman.Get<StageHand>().LoadGameOnTabletop(new DefaultGamePersistenceProvider());
        }

        public async void LoadGame(GamePersistenceProvider gamePersistenceProvider)
        {
            Watchman.Get<Heart>().Metapause();
            Watchman.Get<LocalNexus>().DisablePlayerInput(0f);

            try
            {
                gamePersistenceProvider.DepersistGameState();

                var gameState = gamePersistenceProvider.RetrievePersistedGameState();
                //overwrite current character in stable with most up to date game state. This may have been retrieved from disk, or built from a legacy with freshGameStateProvider
                gameState.MostRecentCharacterCommand().ExecuteToProtagonist(Watchman.Get<Stable>());


                var protag = Watchman.Get<Stable>().Protag();
                if(protag.State==CharacterState.Extinct)
                {
                    Watchman.Get<StageHand>().LegacyChoiceScreen();
                    return;
                }
                else
                    PopulateTabletop(protag, gameState);
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

            Watchman.Get<LocalNexus>().EnablePlayerInput();
            Watchman.Get<Heart>().Unmetapause();
            //set game speed as appropriate to the provider (usually paused, but not for a fresh or restarting game)
            Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 2, GameSpeed = gamePersistenceProvider.GetDefaultGameSpeed(), WithSFX = false });

        }

        private static void PopulateTabletop(Character protag, PersistedGameState gameState)
        {
            Watchman.Get<TabletopBackground>().ShowTabletopFor(protag.ActiveLegacy);

            gameState.RootPopulationCommand.Execute(new Context(Context.ActionSource.Loading));


            foreach (var n in gameState.NotificationCommands)
            {
                Watchman.Get<Concursum>()
                    .ShowNotification(new NotificationArgs(n.Notification.Title,
                        n.Notification
                            .Description)); //ultimately, I'd like the float note to be a token, too - we're using AddCommand here currently just as a holder for the strings
            }

            //Start chronicling the character we just loaded on to the tabletop
            Watchman.Get<Chronicler>().ChronicleCharacter(protag);

            Watchman.Get<StatusBar>().AttachToCharacter(protag);
        }

        private static async Task SaveRestartState()
        {
            var restartingGameStateProvider = new RestartingGameProvider();
       restartingGameStateProvider.Encaust(Watchman.Get<Stable>(), FucineRoot.Get(),Watchman.Get<Xamanek>() );
            var saveTask = restartingGameStateProvider.SerialiseAndSaveAsync();
            var result = await saveTask;
        }



        public async Task<bool> TryDefaultSave()
        {
            if (DefaultSaveInProgress)
                return false;
            DefaultSaveInProgress = true;

            var notifier = Watchman.Get<Notifier>();
            if(notifier==null)
                NoonUtility.LogWarning("Can't find notifier for saving messages");

            try
            {

                if (!gameInSaveableState())
                    return false;
     
                var game = new DefaultGamePersistenceProvider();
                game.Encaust(Watchman.Get<Stable>(), FucineRoot.Get(), Watchman.Get<Xamanek>());
                var saveTask = game.SerialiseAndSaveAsync();
                var result = await saveTask;
                DefaultSaveInProgress = false;

                return result;
            }
            catch (Exception e)
            {
                var notifierArgs = new CustomNotificationWindowArgs();
                notifierArgs.WindowId = CustomNotificationWindowId.ShowSaveError;
                notifierArgs.AdditionalText = $"\n'<b>{e.Message}</b>'";
                notifier.ShowCustomWindow(notifierArgs);
                DefaultSaveInProgress = false;
                return false;
            }
        }

        private bool gameInSaveableState()
        {
            var numa = Watchman.Get<Numa>();
            if (numa == null)
                return true;

            return !numa.IsOtherworldActive();
        }

        

        public async void EndGame(Ending ending, Token focusOnToken)
        {
            //chronicle ending
            var chronicler = Watchman.Get<Chronicler>();
            chronicler.ChronicleGameEnd(Watchman.Get<HornedAxe>().GetRegisteredSituations(), Watchman.Get<HornedAxe>().GetSpheres(), ending);

            //Mark character as defunct
            Watchman.Get<Stable>().Protag().EnactEnding(ending);
            
            //stop everything
            Watchman.Get<Heart>().Metapause();
            //save game, so our chronicled information is persisted
            var saveResult = await TryDefaultSave();


            //stop current music clip
            Watchman.Get<BackgroundMusic>().FadeToSilence(3f);
            InstantiateCSEndingEffect(ending, focusOnToken);
            Watchman.Get<LocalNexus>().DisablePlayerInput(10f);
            Watchman.Get<LocalNexus>().AbortEvent.AddListener(FinalTransitionToEndingScreen);
            Watchman.Get<CamOperator>().PointAtTableLevelWithZoomFactor(focusOnToken.transform.position,0.6f,1.52f, FinalTransitionToEndingScreen);

        }



        public void FinalTransitionToEndingScreen()
        {
            if(fadeToEndingCoroutine==null) //otherwise if we hit escape in the middle of a fadeout, we get two ending screen loads
                fadeToEndingCoroutine=StartCoroutine(FadeToEndingScreen());
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

            Debug.Log(effectName);
            
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
