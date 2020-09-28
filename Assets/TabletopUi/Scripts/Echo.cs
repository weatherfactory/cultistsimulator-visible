using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine;

namespace Assets.TabletopUi.Scripts
{


    public class Echo: MonoBehaviour
    {
        private GameSpeedState locallyTrackedGameSpeedState = new GameSpeedState(); //so we can check if we're pausing or unpausing

        //responds to events that should cause SFX to fire
        public void RespondToSpeedControlCommand(SpeedControlEventArgs args)
        {
            if (!args.WithSFX)
                return;

            bool pausedBeforeInteraction = locallyTrackedGameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Paused;
            locallyTrackedGameSpeedState.SetGameSpeedCommand(args.ControlPriorityLevel, args.GameSpeed);

            bool pausedAfterInteraction = locallyTrackedGameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Paused;

            if (!pausedBeforeInteraction && pausedAfterInteraction)
                SoundManager.PlaySfx("UIPauseStart");
            else if (pausedBeforeInteraction && !pausedAfterInteraction)
                SoundManager.PlaySfx("UIPauseEnd");
            else
                SoundManager.PlaySfx("UIButtonClick");
        }

    }
}
