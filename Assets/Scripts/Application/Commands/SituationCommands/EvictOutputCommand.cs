using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using UnityEditor;

namespace SecretHistories.Assets.Scripts.Application.Commands.SituationCommands
{
    public class EvictOutputCommand : ISituationCommand
    {

        public bool IsValidForState(StateEnum forState)
        {

            return forState == StateEnum.Complete;
        }

        public bool IsObsoleteInState(StateEnum forState)
        {
            return forState == StateEnum.Unstarted;
        }

        public bool Execute(Situation situation)
        {
            var results = situation.GetElementTokens(SphereCategory.Output);
            
            foreach (var item in results)
            {
                item.Unshroud(true);
                item.GoAway(new Context(Context.ActionSource.PlayerDumpAll));
            }
            // Only play collect all if there's actually something to collect 
            // Only play collect all if it's not transient - cause that will retire it and play the retire sound
            // Note: If we collect all from the window we also get the default button sound in any case.
            if (results.Any())
                SoundManager.PlaySfx("SituationCollectAll");
            else if (situation.Verb.Spontaneous)
                SoundManager.PlaySfx("SituationTokenRetire");
            else
                SoundManager.PlaySfx("UIButtonClick");

            return true;


        }


    }
}
