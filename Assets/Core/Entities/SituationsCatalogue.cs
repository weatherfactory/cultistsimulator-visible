using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;

namespace Assets.Core.Entities
{
    public class SituationsCatalogue
    {
        private Dictionary<string, SituationBuilder> _builders;
        private  List<Situation> _currentSituations;
        

        public SituationsCatalogue()
        {
           Reset();
            
        }

        
        public void Reset()
        {
            _builders = new Dictionary<string, SituationBuilder>();
            _currentSituations = new List<Situation>();
        }

        public void RegisterBuilder(string forSpecies, SituationBuilder builder)
        {
            if(!_builders.ContainsKey(forSpecies))
                _builders.Add(forSpecies,builder);
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
        
        
        public Situation GetOpenSituation()
        {
            return GetRegisteredSituations().FirstOrDefault(s => s.IsOpen);
        }

        public Situation CreateSituation(SituationCreationCommand command)
        {
            var builder = _builders[command.Verb.SpeciesId];
            return builder.CreateSituation(command);
        }

        //public IEnumerable<IArtAnimatableToken> GetAnimatables()
        //{
        //    var situationTokens = GetRegisteredSituations().Select(s => s.situationAnchor as IArtAnimatableToken);

        //    return situationTokens.Where(s => s.CanAnimateArt());

    }
}
