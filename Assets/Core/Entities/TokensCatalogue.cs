using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.TabletopUi;

namespace Assets.Core.Entities
{
   public class TokensCatalogue
    {
        public List<SituationController> CurrentSituationControllers;

        public TokensCatalogue()
        {
            CurrentSituationControllers=new List<SituationController>();
        }

        public List<SituationController> GetRegisteredSituations()
        {
            return CurrentSituationControllers.ToList();
        }

        public void RegisterSituation(SituationController controller)
        {
            CurrentSituationControllers.Add(controller);
        }

        public void DeregisterSituation(SituationController situationController)
        {
            CurrentSituationControllers.Remove(situationController);
        }
    }
}
