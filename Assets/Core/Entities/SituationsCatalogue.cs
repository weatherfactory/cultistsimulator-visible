using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;

namespace Assets.Core.Entities
{
    public class SituationsCatalogue
    {
        private readonly List<SituationController> CurrentSituationControllers;

        public SituationsCatalogue()
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
        
        public SituationController GetOpenSituation()
        {
            return GetRegisteredSituations().FirstOrDefault(s => s.IsOpen);
    }
    }
}
