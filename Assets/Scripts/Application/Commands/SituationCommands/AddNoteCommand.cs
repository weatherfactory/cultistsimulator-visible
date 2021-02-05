using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.UI;

namespace Assets.Scripts.Application.Commands.SituationCommands
{
   public class AddNoteCommand: ISituationCommand
   {
       public CommandCategory CommandCategory => CommandCategory.Notes;
        public bool Execute(Situation situation)
        {
            var noteElementId = Watchman.Get<Compendium>().GetSingleEntity<Dictum>().NoteElementId;
            var notesDominion = situation.GetSituationDominionsForCommandCategory(this.CommandCategory).FirstOrDefault();

            return true;
        }
    }
}
