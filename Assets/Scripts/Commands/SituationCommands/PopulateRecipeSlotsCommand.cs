using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.States;

namespace Assets.Scripts.Commands.SituationCommands
{
  public  class PopulateRecipeSlotsCommand: ISituationCommand
    {
        private List<SlotSpecification> _populateWithSlots = new List<SlotSpecification>();

        public CommandCategory CommandCategory => CommandCategory.RecipeSlots;
        public PopulateRecipeSlotsCommand(List<SlotSpecification> populateWithSlots)
        {
            _populateWithSlots.AddRange(populateWithSlots);
        }



        public bool Execute(Situation situation)
        {
            if (_populateWithSlots.Count > 0) //only execute if there are any relevant slot instructions. We don't want to clear existing slots with a recipe that doesn't specify them
                //this may be irrelevant if we only add a command when we need one
            {
                var attachment = situation.GetSituationAttachmentsForCommandCategory(this.CommandCategory).FirstOrDefault();
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
