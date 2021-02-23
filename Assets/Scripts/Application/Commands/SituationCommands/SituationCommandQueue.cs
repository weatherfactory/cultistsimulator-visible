using System.Collections.Generic;
using System.Linq;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.States;

namespace SecretHistories.Core
{

    public class SituationCommandQueue
    {
        private readonly List<ISituationCommand> _commands=new List<ISituationCommand>();

        public void ExecuteCommandsFor(List<StateEnum> forStates, Situation situation)
        {
            foreach (var command in new List<ISituationCommand>(_commands))
            {
                if (forStates.Intersect(command.ValidForStates).Any())
                {
                    bool executed = command.Execute(situation);
                    if (executed)
                        MarkCommandCompleted(command);
                }
            }
        }


        public void ExecuteCommandsFor(StateEnum forState,Situation situation)
        {
            foreach(var command in new List<ISituationCommand>(_commands))
            {
                if (command.ValidForStates.Contains(forState))
                {
                    bool executed = command.Execute(situation);
                    if (executed)
                        MarkCommandCompleted(command);
                }
            }
        }

        public void ExecuteAllCommands(Situation situation)
        {
            foreach (var command in new List<ISituationCommand>(_commands))
            {
                    bool executed = command.Execute(situation);
                    if (executed)
                        MarkCommandCompleted(command);
            }
        }

        public void AddCommand(ISituationCommand command)
        {
            _commands.Add(command);
        }

        public void AddCommandsFrom(SituationCommandQueue existingQueue)
        {
            foreach (var c in existingQueue.GetCurrentCommandsAsList())
                AddCommand(c);
        }

        public void MarkCommandCompleted(ISituationCommand command)
        {
            _commands.Remove(command);

        }

        public List<ISituationCommand> GetCurrentCommandsAsList()
        {
            return new List<ISituationCommand>(_commands);
        }

    }
}