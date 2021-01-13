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
  public  class PopulateSlotsCommand: ISituationCommand
    {
        private List<SphereSpec> _populateWithSlots = new List<SphereSpec>();

        public CommandCategory CommandCategory => CommandCategory.RecipeThresholds;
        public PopulateSlotsCommand(List<SphereSpec> populateWithSlots)
        {
            _populateWithSlots.AddRange(populateWithSlots);
        }


        public bool Execute(Situation situation)
        {
            if (_populateWithSlots.Count > 0) //only execute if there are any relevant slot instructions. We don't want to clear existing slots with a recipe that doesn't specify them
                //this may be irrelevant if we only add a command when we need one
            {
                var attachment = situation.GetSituationDominionsForCommandCategory(this.CommandCategory).FirstOrDefault();
                if(attachment!=null)
                {
                    attachment.ClearThresholds();
                    foreach (var spec in _populateWithSlots)
                        attachment.CreateThreshold(spec);
                    return true;
                }
            }

            return false;
        }
        
    }
}
