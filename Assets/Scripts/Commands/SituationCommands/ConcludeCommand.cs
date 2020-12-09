using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.States;

namespace Assets.Scripts.Commands.SituationCommands
{
   public class ConcludeCommand: ISituationCommand
   {
       public CommandCategory CommandCategory => CommandCategory.Output;

        public bool Execute(Situation situation)
        {
            var results = situation.GetElementTokens(SphereCategory.Output);
            foreach (var item in results)
                item.ReturnToTabletop(new Context(Context.ActionSource.PlayerDumpAll));

            // Only play collect all if there's actually something to collect 
            // Only play collect all if it's not transient - cause that will retire it and play the retire sound
            // Note: If we collect all from the window we also get the default button sound in any case.
            if (results.Any())
                SoundManager.PlaySfx("SituationCollectAll");
            else if (situation.Verb.Transient)
                SoundManager.PlaySfx("SituationTokenRetire");
            else
                SoundManager.PlaySfx("UIButtonClick");

            situation.TransitionToState(new UnstartedState());
            return true;
        }
    }
}
