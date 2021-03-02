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
using SecretHistories.Services;

using UnityEngine;

namespace SecretHistories.Constants
{

    public enum NonSaveableType
    {
        Drag, // Cannot save because held card gets lost
        Mansus, // Cannot save by design
        Greedy, // Cannot save during Magnet grab (spec fix for #1253)
        WindowAnim, // Cannot save during situation window open
        NumNonSaveableTypes
    };

    public class Autosaver: MonoBehaviour, ISettingSubscriber
    {

        [SerializeField] private AutosaveWindow _autosaveNotifier;

        static private bool[] isInNonSaveableState = new bool[(int)NonSaveableType.NumNonSaveableTypes];



        private float
            housekeepingTimer; // Now a float so that we can time autosaves independent of Heart.Beat - CP

        private float AUTOSAVE_INTERVAL = 300.0f;




        private bool IsSafeToAutosave()
        {
            for (int i = 0; i < (int)NonSaveableType.NumNonSaveableTypes; i++)
            {
                if (isInNonSaveableState[i])
                    return false;
            }
            return true;
        }


        private void RemoveAllNonsaveableFlags()  // For use when we absolutely, definitely want to restore autosave permission - CP
        {
            for (int i = 0; i < (int)NonSaveableType.NumNonSaveableTypes; i++)
            {
                isInNonSaveableState[i] = false;
            }
        }


        protected void SetAutosaveInterval(float minutes)
        {
            AUTOSAVE_INTERVAL = minutes * 60.0f;
        }

        public void WhenSettingUpdated(object newValue)
        {
            SetAutosaveInterval(newValue is float ? (float)newValue : 0);
        }



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
