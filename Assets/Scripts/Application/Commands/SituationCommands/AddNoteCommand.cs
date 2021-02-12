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

namespace Assets.Scripts.Application.Commands.SituationCommands
{
   public class AddNoteCommand: IAffectsTokenCommand
   {

       public CommandCategory CommandCategory => CommandCategory.Notes;
       private readonly string _label;
       private readonly string _description;
       private readonly Context _context;

       public AddNoteCommand(string label,string description,Context context)
       {
           _label = label;
           _description = description;
           _context = context;
       }

       public bool ExecuteOn(Token token)
       {
           return false;
       }

       public bool ExecuteOn(ITokenPayload payload)
       {
           return payload.ReceiveNote(_label,_description,_context);
       }
   }

   }
