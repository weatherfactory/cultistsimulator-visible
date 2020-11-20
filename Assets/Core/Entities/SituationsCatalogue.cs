using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Enums;
using Assets.Core.NullObjects;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;

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

        public IEnumerable<Situation> GetSituationWithVerbOfType(Type verbType)
        {
            return _currentSituations.Where(situation => situation.Verb.GetType() == verbType);
        }
        
        
        public Situation GetOpenSituation()
        {
            return GetRegisteredSituations().FirstOrDefault(s => s.IsOpen);
        }


        public Situation TryBeginNewSituation(SituationCreationCommand scc, List<Token> withStacksInStorage)
        {
            if (scc.Recipe == null)
                throw new ApplicationException("DON'T PASS AROUND SITUATIONCREATIONCOMMANDS WITH RECIPE NULL");
            if (withStacksInStorage == null)
                throw new ApplicationException("WITHSTACKSINSTORAGE SHOULD NEVER BE NULL");

            //if new situation is beginning with an existing verb: do not action the creation.
            //oh: I could have an scc property which is a MUST CREATE override


            var registeredSituations = GetRegisteredSituations();

            if (registeredSituations.Exists(rs => !scc.Verb.CreationAllowedWhenAlreadyExists(rs)))
            {
                NoonUtility.Log("Tried to create " + scc.Recipe.Id + " for verb " + scc.Recipe.ActionId + " but that verb is already active.");
                //end execution here
                return new NullSituation();
            }

    
            var builder = Registry.Get<SituationBuilder>();
            var situation =builder.CreateSituation(scc);

            situation.ExecuteHeartbeat(0f);


            //if there's been (for instance) an expulsion, we now want to add the relevant stacks to this situation
            if (withStacksInStorage.Any())
                situation.AcceptTokens(SphereCategory.SituationStorage, withStacksInStorage);

            return situation;

        }

        //public IEnumerable<Token> GetAnimatables()
        //{
        //    var situationTokens = GetRegisteredSituations().Select(s => s.situationAnchor as Token);

        //    return situationTokens.Where(s => s.CanAnimateArt());

    }
}
