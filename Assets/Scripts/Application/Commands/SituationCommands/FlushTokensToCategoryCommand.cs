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
        public List<StateEnum> GetStatesCommandIsValidFor() => new List<StateEnum>();

        public FlushTokensToCategoryCommand(SphereCategory fromCategory,SphereCategory toCategory,StateEnum onState)
        {
            _fromCategory = fromCategory;
            _toCategory = toCategory;
            GetStatesCommandIsValidFor().Add(onState);
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

            toSphere.AcceptTokens(situation.GetTokens(_fromCategory),
                new Context(Context.ActionSource.TokenMigration));

            return true;
        }
    }
}
