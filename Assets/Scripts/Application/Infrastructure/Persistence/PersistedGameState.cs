using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;

namespace SecretHistories.Infrastructure.Persistence
{
    public class PersistedGameState: IEncaustment
    {
        public List<CharacterCreationCommand> CharacterCreationCommands;
        public List<TokenCreationCommand> TokenCreationCommands;
        public List<AddNoteCommand> NotificationCommands;

        public PersistedGameState()
        {

            CharacterCreationCommands=new List<CharacterCreationCommand>();
            TokenCreationCommands=new List<TokenCreationCommand>();
            NotificationCommands=new List<AddNoteCommand>();
        }
    }
}
