﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Commands.TokenEffectCommands;
using SecretHistories.Entities;

namespace SecretHistories.Infrastructure.Persistence
{
    public class PersistedGameState: IEncaustment
    {
        public List<CharacterCreationCommand> CharacterCreationCommands;
        public RootPopulationCommand RootPopulationCommand;
        public PopulateXamanekCommand PopulateXamanekCommand;
        public List<AddNoteToTokenCommand> NotificationCommands;
        public PersistedGameState()
        {

            CharacterCreationCommands=new List<CharacterCreationCommand>();
            RootPopulationCommand=new RootPopulationCommand();
            PopulateXamanekCommand = new PopulateXamanekCommand();
            NotificationCommands =new List<AddNoteToTokenCommand>();
        }

        public CharacterCreationCommand MostRecentCharacterCommand()
        {
            return (CharacterCreationCommands.OrderByDescending(c => c.DateTimeCreated).FirstOrDefault());
        }

        public static PersistedGameState ForLegacy(Legacy startingLegacy,Dictionary<string,string> historyRecordsFromPreviousCharacter)
        {
            var state = new PersistedGameState
            {
                RootPopulationCommand = RootPopulationCommand.RootCommandForLegacy(startingLegacy),
                PopulateXamanekCommand = PopulateXamanekCommand.ForLegacy()
            };
            
            var cc = CharacterCreationCommand.IncarnateFromLegacy(startingLegacy, historyRecordsFromPreviousCharacter);
            state.CharacterCreationCommands.Add(cc);
            var note=new Notification(startingLegacy.Label, startingLegacy.Description);
            var notificationCommand = new AddNoteToTokenCommand(note, new Context(Context.ActionSource.Loading));
            state.NotificationCommands.Add(notificationCommand);

            return state;
        }
    }
}
