using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Interfaces;

namespace Assets.Core.Entities
{
    public class SituationsCatalogue
    {
        private readonly List<Situation> _currentSituations;

        public SituationsCatalogue()
        {
            _currentSituations = new List<Situation>();
        }

        public List<Situation> GetRegisteredSituations()
        {
            return _currentSituations.ToList();
        }


        public void RegisterSituation(Situation situation)
        {
            _currentSituations.Add(situation);
        }

        public void DeregisterSituation(Situation situation)
        {
            _currentSituations.Remove(situation);
        }
        
        public SituationController GetOpenSituation()
        {
            return GetRegisteredSituations().FirstOrDefault(s => s.IsOpen);
    }

        public IEnumerable<IAnimatableToken> GetAnimatables()
        {
            var situationTokens = GetRegisteredSituations().Select(s => s.situationAnchor as IAnimatableToken);

            return situationTokens.Where(s => s.CanAnimate());
        }
    }
}
