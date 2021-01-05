using System.Collections.Generic;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.States;

namespace SecretHistories.Core
{

    public class SituationCommandQueue
    {
        private readonly List<ISituationCommand> _commands=new List<ISituationCommand>();

        public void ExecuteCommandsFor(CommandCategory forActiveOnAttachment,Situation situation)
        {
            foreach(var command in new List<ISituationCommand>(_commands))
            {
                if (command.CommandCategory == forActiveOnAttachment)
                {
                    bool executed = command.Execute(situation);
                    if (executed)
                        MarkCommandCompleted(command);
                }
            }
        }

        public void AddCommand(ISituationCommand command)
        {
            _commands.Add(command);
        }

        public void MarkCommandCompleted(ISituationCommand command)
        {
            _commands.Remove(command);

        }

    }
}