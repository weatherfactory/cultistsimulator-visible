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
        private readonly List<SituationController> _currentSituationControllers;

        public SituationsCatalogue()
        {
            _currentSituationControllers=new List<SituationController>();
        }

        public List<SituationController> GetRegisteredSituations()
        {
            return _currentSituationControllers.ToList();
        }

        public void RegisterSituation(SituationController controller)
        {
            _currentSituationControllers.Add(controller);
        }

        public void DeregisterSituation(SituationController situationController)
        {
            _currentSituationControllers.Remove(situationController);
        }
        
        public SituationController GetOpenSituation()
        {
            return GetRegisteredSituations().FirstOrDefault(s => s.IsOpen);
    }
    }
}
