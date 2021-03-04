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
            var toSphere = situation.GetSingleSphereByCategory(_toCategory);

            

            //now we're safely started on the migration, consume any tokens in Consuming thresholds
            foreach (var fromSphere in situation.GetSpheresByCategory(_fromCategory))
            {
                if (fromSphere.GoverningSphereSpec.Consumes)
                    fromSphere.RetireAllTokens();
            }

            if (toSphere == null)
            {
                NoonUtility.LogWarning($"We're about to try to flush tokens to sphere category {_toCategory}, but there aren't any. Execution won't occur.");
                return false;
            }
            toSphere.AcceptTokens(situation.GetTokens(_fromCategory),
                new Context(Context.ActionSource.TokenMigration));

            return true;
        }
    }
}
