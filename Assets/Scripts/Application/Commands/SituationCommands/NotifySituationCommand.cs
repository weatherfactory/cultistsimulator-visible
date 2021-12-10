using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Commands.TokenEffectCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;

namespace SecretHistories.Assets.Scripts.Application.Commands.SituationCommands
{


    public class NotifySituationCommand: ISituationCommand
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public NotifySituationCommand()
        {

        }

        public NotifySituationCommand(string title,string description, Context context):base()
        {
            Title = title;
            Description = description;
        }

        public bool Execute(Situation situation)
        {
            var notification = new Notification(Title, Description);
            var affectsTokenNoteCommand=new AddNoteToTokenCommand(notification, Context.Unknown());
            situation.Token.ExecuteTokenEffectCommand(affectsTokenNoteCommand);
            return true;
        }

        public bool IsValidForState(StateEnum forState)
        {
            return true;
        }

        public bool IsObsoleteInState(StateEnum forState)
        {
            return false;
        }
    }
}
