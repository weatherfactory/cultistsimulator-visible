using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;

namespace SecretHistories.Assets.Scripts.Application.Commands.SituationCommands
{
   public class MigrateTokensInsideSituation: ISituationCommand
    {
        private readonly SphereCategory _fromCategory;
        private readonly SphereCategory _toCategory;
        public CommandCategory CommandCategory { get; }

        public MigrateTokensInsideSituation(SphereCategory fromCategory,SphereCategory toCategory,CommandCategory commandCategory)
        {
            _fromCategory = fromCategory;
            _toCategory = toCategory;
            CommandCategory = commandCategory;
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
