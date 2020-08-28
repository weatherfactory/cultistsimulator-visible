using System.Collections.Generic;
using Noon;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class GameSpeedState
    {
        private const int levels = 3;
        private Dictionary<int,GameSpeed> CurrentGameSpeedCommands=new Dictionary<int, GameSpeed>();

        public GameSpeedState()
        {
            //1: speed control
            //2: pause state. Overrides speed control, we revert to speed control
            //3: game locked - can't un-pause with buttons or keystrokes

            for (int l = 1; l <=levels; l++)
            {
                CurrentGameSpeedCommands.Add(l, GameSpeed.Unspecified);
            }

        }

        public void SetGameSpeedCommand(int commandPriority, GameSpeed speed)
        {
            if (commandPriority > levels || commandPriority<0)
            {
                NoonUtility.Log("Can't set a speed command priority to " + commandPriority);
                return;
            }

            //consider a line here to flip pause if toggled at the same command level

            CurrentGameSpeedCommands[commandPriority] = speed;
        }

        public GameSpeed GetEffectiveGameSpeed()
        {
            GameSpeed effectiveSpeed=GameSpeed.Unspecified;
            foreach (var c in CurrentGameSpeedCommands)
            {
                if (c.Value != GameSpeed.Unspecified)
                    effectiveSpeed = c.Value;
            }

            return effectiveSpeed;
        }

    }
}