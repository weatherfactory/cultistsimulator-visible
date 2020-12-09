using System.Collections.Generic;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.States;
using NUnit.Framework;

namespace Assets.Core
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