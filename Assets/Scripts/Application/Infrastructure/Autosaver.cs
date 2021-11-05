using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.Services;

using UnityEngine;

namespace SecretHistories.Infrastructure
{


    public class Autosaver: MonoBehaviour, ISettingSubscriber
    {


        [SerializeField]  private float
            housekeepingTimer; // Now a float so that we can time autosaves independent of Heart.Beat - CP

      [SerializeField] private float AUTOSAVE_INTERVAL = 300.0f;
      [SerializeField] private CanvasGroupFader AutosaveIndicator;

      [SerializeField] private bool SaveWhenEditorStops;


        private GameGateway gameGateway;
      private Heart heart;

      public void Awake()
      {
         Initialise();

      }

      public void Initialise()
      {
          AutosaveIndicator.Hide();

            var registry = new Watchman();
          registry.Register(this);


          var autosaveSetting = Watchman.Get<Compendium>().GetEntityById<Setting>(NoonConstants.AUTOSAVEINTERVAL);
          if (autosaveSetting != null)
          {
              autosaveSetting.AddSubscriber(this);
              WhenSettingUpdated(autosaveSetting.CurrentValue);
          }
          else
              NoonUtility.Log("Missing setting entity: " + NoonConstants.AUTOSAVEINTERVAL);


          gameGateway = Watchman.Get<GameGateway>();
          if(gameGateway==null)
              NoonUtility.LogWarning("Autosaver can't find GameGateway; autosave won't run.");

          heart = Watchman.Get<Heart>();
          if (heart == null)
              NoonUtility.LogWarning("Autosaver can't find Heart, which will be bad news.");
        }


        protected void SetAutosaveInterval(float minutes)
        {
            AUTOSAVE_INTERVAL = minutes * 60.0f;
        }

        public void WhenSettingUpdated(object newValue)
        {
            SetAutosaveInterval(newValue is float ? (float)newValue : 0);
        }

        public void Update()
        {
            if(!heart.Metapaused)
                housekeepingTimer += Time.deltaTime;

            if (housekeepingTimer > AUTOSAVE_INTERVAL)
            {
                housekeepingTimer = 0f;
                  TryAutosave();
            }
        }

        public async void TryAutosave()
        {
            AutosaveIndicator.Show();

         heart.Metapause();
            Watchman.Get<LocalNexus>().DisablePlayerInput(0f);


                var saveResult = await gameGateway.TryDefaultSave();
                AutosaveIndicator.Hide(); //always hide, succeed or fail
                //but stay paused and disabled if the save went wrong

                if (saveResult)
                {
                    heart.Unmetapause();
                    Watchman.Get<LocalNexus>().EnablePlayerInput();
                }
        }

        public async void OnApplicationQuit()
        {
            if (Application.isEditor && !SaveWhenEditorStops)
                return; 
            var saveResult = await gameGateway.TryDefaultSave();
        }

        //Old code below for history and reference
        //public async Task<bool> SaveGameAsync(bool withNotification)
        //{
        //    return false;
        //if (!IsSafeToAutosave())
        //{
        //    NoonUtility.Log("Unsafe to autosave: returning", 0, VerbosityLevel.SystemChatter);
        //    return false;
        //}

        //if (withNotification && _autosaveNotifier != null)
        //{
        //    NoonUtility.Log("Displaying autosave notification", 0, VerbosityLevel.SystemChatter);

        //    //_notifier.ShowNotificationWindow("SAVED THE GAME", "BUT NOT THE WORLD");
        //    _autosaveNotifier.SetDuration(3.0f);
        //    _autosaveNotifier.Show();
        //}


        //Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.Paused, WithSFX = false });
        //NoonUtility.Log("Paused game for saving", 0, VerbosityLevel.SystemChatter);


        //try
        //{
        //    ITableSaveState tableSaveState = new TableSaveState(_tabletop.GetElementStacks(), Registry.Get<SituationsCatalogue>().GetRegisteredSituations(), Registry.Get<MetaInfo>());
        //    var saveTask = Registry.Get<GameSaveManager>().SaveActiveGameAsync(tableSaveState, Registry.Get<Character>(), source);
        //    NoonUtility.Log("Beginning save", 0, VerbosityLevel.SystemChatter);
        //    bool success = await saveTask;
        //    NoonUtility.Log($"Save status: {success}", 0, VerbosityLevel.SystemChatter);

        //}
        //catch (Exception e)
        //{
        //    GameSaveManager.ShowSaveError();
        //    GameSaveManager.saveErrorWarningTriggered = true;
        //    Debug.LogError("Failed to save game (see exception for details)");
        //    Debug.LogException(e);
        //}

        //Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.DeferToNextLowestCommand, WithSFX = false });
        //NoonUtility.Log("Unpausing game after saving", 0, VerbosityLevel.SystemChatter);




        //if (GameSaveManager.saveErrorWarningTriggered)  // Do a full pause after resuming heartbeat (to update UI, SFX, etc)
        //{
        //    NoonUtility.Log("Triggering save error warning", 0, VerbosityLevel.SystemChatter);

        //    // only pause if we need to (since it triggers sfx)
        //    Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
        //    { ControlPriorityLevel = 1, GameSpeed = GameSpeed.Paused, WithSFX = false });

        //    GameSaveManager.saveErrorWarningTriggered = false;  // Clear after we've used it
        //}

        //return true;
        //    }



    }
}
