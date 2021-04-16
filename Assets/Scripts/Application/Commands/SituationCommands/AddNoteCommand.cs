using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;

namespace SecretHistories.Commands.SituationCommands
{
   public class AddNoteCommand: IAffectsTokenCommand
   {

       public List<StateEnum> ValidForStates => new List<StateEnum> { StateEnum.Unstarted,StateEnum.Complete,StateEnum.Halting,StateEnum.Ongoing,StateEnum.RequiringExecution,StateEnum.Inchoate };
        public readonly string Label;
       public readonly string Description;
       private readonly Context _context;

       public AddNoteCommand(string label,string description,Context context)
       {
           Label = label;
           Description = description;
           _context = context;
       }

       public bool ExecuteOn(Token token)
       {
           return false;
       }

       public bool ExecuteOn(ITokenPayload payload)
       {
           _context.Metafictional = true;
           var note=new Notification(Label,Description);
           return payload.ReceiveNote(note,_context);
       }
   }

   }
