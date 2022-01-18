using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Infrastructure
{
    public abstract class AbstractTokenSetupChamberlain
    {
        public abstract List<TokenCreationCommand> GetTokenCreationCommandsToEnactLegacy(Legacy forLegacy);
    }
}
