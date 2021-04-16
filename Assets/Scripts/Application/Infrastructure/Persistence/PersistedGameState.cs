﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;

namespace SecretHistories.Infrastructure.Persistence
{
    public class PersistedGameState: IEncaustment
    {
        public List<CharacterCreationCommand> CharacterCreationCommands;
        public RootPopulationCommand RootPopulationCommand;
        public List<AddNoteCommand> NotificationCommands;
        public PersistedGameState()
        {

            CharacterCreationCommands=new List<CharacterCreationCommand>();
            RootPopulationCommand=new RootPopulationCommand();
            NotificationCommands =new List<AddNoteCommand>();
        }


        public static PersistedGameState ForLegacy(Legacy startingLegacy)
        {
            var state = new PersistedGameState
            {
                RootPopulationCommand = RootPopulationCommand.RootCommandForLegacy(startingLegacy)
            };
            
            var cc = CharacterCreationCommand.IncarnateFromLegacy(startingLegacy);
            state.CharacterCreationCommands.Add(cc);
            var note=new Notification(startingLegacy.Label, startingLegacy.Description);
            var notificationCommand = new AddNoteCommand(note, new Context(Context.ActionSource.Loading));
            state.NotificationCommands.Add(notificationCommand);

            return state;
        }
    }
}
