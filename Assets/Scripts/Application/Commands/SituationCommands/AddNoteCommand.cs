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
using SecretHistories.Fucine;
using SecretHistories.UI;

namespace SecretHistories.Commands.SituationCommands
{
   public class AddNoteCommand: IAffectsTokenCommand
   {

       public List<StateEnum> ValidForStates => new List<StateEnum> { StateEnum.Unstarted,StateEnum.Complete,StateEnum.Halting,StateEnum.Ongoing,StateEnum.RequiringExecution,StateEnum.Inchoate };
       public INotification Notification { get; protected set; }
       private readonly Context _context;

       public AddNoteCommand(INotification notification,Context context)
       {
           Notification = notification;
           _context = context;
       }

       public bool ExecuteOn(Token token)
       {
           return false;
       }

       public bool ExecuteOn(ITokenPayload payload)
       {
           _context.Metafictional = true;
           return payload.ReceiveNote(Notification, _context);
       }
   }

   }
