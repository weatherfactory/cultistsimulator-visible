using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Commands.SituationCommands;
using SecretHistories.Commands;

namespace SecretHistories.Infrastructure.Persistence
{
    public class PersistedGameState
    {
        public List<CharacterCreationCommand> CharacterCreationCommands;
        public List<TokenCreationCommand> TokenCreationCommands;

        public PersistedGameState()
        {
            CharacterCreationCommands=new List<CharacterCreationCommand>();
            TokenCreationCommands=new List<TokenCreationCommand>();
            TokenCreationCommands=new List<TokenCreationCommand>();
        }
    }
}
