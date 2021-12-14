using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;

namespace SecretHistories.Assets.Scripts.Application.Infrastructure
{
    public class BHTokenSetupChamberlain: AbstractTokenSetupChamberlain
    {
        public override List<TokenCreationCommand> GetTokenCreationCommandsToEnactLegacy(Legacy forLegacy)
        {
            throw new NotImplementedException();
        }
    }
}
