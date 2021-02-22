using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.States;

namespace SecretHistories.Commands.SituationCommands
{
  public  class PopulateDominionSpheresCommand: ISituationCommand
    {
        private List<SphereSpec> _populateWith = new List<SphereSpec>();

        public CommandCategory CommandCategory { get; }

        public PopulateDominionSpheresCommand(CommandCategory commandCategory, SphereSpec populateWith)
        {
            _populateWith.Add(populateWith);
            CommandCategory = commandCategory;
        }

        public PopulateDominionSpheresCommand(CommandCategory commandCategory, List<SphereSpec> populateWith)
        {
            _populateWith.AddRange(populateWith);
            CommandCategory = commandCategory;
        }


        public bool Execute(Situation situation)
        {
            if (_populateWith.Count > 0) //only execute if there are any relevant slot instructions. We don't want to clear existing slots with a recipe that doesn't specify them
                //this may be irrelevant if we only add a command when we need one
            {
                var dominion = situation.GetSituationDominionsForCommandCategory(this.CommandCategory).FirstOrDefault();
                if(dominion!=null)
                {
                    dominion.RemoveAllSpheres();
                    foreach (var spec in _populateWith)
                        dominion.CreatePrimarySphere(spec);
                    return true;
                }
            }

            return false;
        }
        
    }
}
