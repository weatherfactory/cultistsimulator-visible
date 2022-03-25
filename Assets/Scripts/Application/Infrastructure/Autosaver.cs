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


   [SerializeField]     private GameGateway gameGateway;
   [SerializeField] private Heart heart;

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

          //Now using direct unity references, since they're all Monobehaviours and execution order can be tricky otherwise
          //gameGateway = Watchman.Get<GameGateway>();
          //if(gameGateway==null)
          //    NoonUtility.LogWarning("Autosaver can't find GameGateway; autosave won't run.");

          //heart = Watchman.Get<Heart>();
          //if (heart == null)
          //    NoonUtility.LogWarning("Autosaver can't find Heart, which will be bad news.");
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
              
                  TryAutosave();
            }
        }

        public async void TryAutosave()
        {
            housekeepingTimer = 0f;
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

        public  void OnApplicationFocus(bool hasFocus)
        {
            if(!hasFocus)
                TryAutosave();

        }



    }
}
