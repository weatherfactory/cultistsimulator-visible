using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;

namespace SecretHistories.Commands.SituationCommands
{
   public class FlushTokensToCategoryCommand: ISituationCommand
    {
        private readonly SphereCategory _fromCategory;
        private readonly SphereCategory _toCategory;

        private readonly List<StateEnum> _statesCommandIsValidFor=new List<StateEnum>();

  


        public bool IsValidForState(StateEnum forState)
        {
            return _statesCommandIsValidFor.Contains(forState);
        }

        public FlushTokensToCategoryCommand(SphereCategory fromCategory,SphereCategory toCategory,StateEnum onState)
        {
            _fromCategory = fromCategory;
            _toCategory = toCategory;
            _statesCommandIsValidFor.Add(onState);
        }


        public bool Execute(Situation situation)
        {
            //Get the destination sphere. We only expect one.
            var toSphere = situation.GetSingleSphereByCategory(_toCategory);

            //if we can't find one, give up.
            if (toSphere == null)
            {
                NoonUtility.LogWarning($"We're about to try to flush tokens to sphere category {_toCategory}, but there aren't any. Execution won't occur.");
                return false;
            }

            var fromSpheres = situation.GetSpheresByCategory(_fromCategory);
            //if this is count=0, it doesn't matter.

            foreach (var fs in fromSpheres)
            {
                toSphere.AcceptTokens(fs.Tokens, new Context(Context.ActionSource.FlushingTokens));
            }

            ////code was originally this
            //toSphere.AcceptTokens(situation.GetTokens(_fromCategory),
            //    new Context(Context.ActionSource.FlushingTokens));


            return true;
        }
    }
}
