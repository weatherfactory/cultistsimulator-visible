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
        private readonly List<SphereSpec> _populateWith = new List<SphereSpec>();

        public CommandCategory CommandCategory => CommandCategory.All;

        public PopulateDominionSpheresCommand(SphereSpec populateWith)
        {
            _populateWith.Add(populateWith);
        }

        public PopulateDominionSpheresCommand(List<SphereSpec> populateWith)
        {
            _populateWith.AddRange(populateWith);
        }


        public bool Execute(Situation situation)
        {
            if (_populateWith.Count > 0) //This means we don't clear existing slots unless there are sl
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
