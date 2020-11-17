using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Enums;
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
        
        
        public Situation GetOpenSituation()
        {
            return GetRegisteredSituations().FirstOrDefault(s => s.IsOpen);
        }


        public void BeginNewSituation(SituationCreationCommand scc, List<Token> withStacksInStorage)
        {
            if (scc.Recipe == null)
                throw new ApplicationException("DON'T PASS AROUND SITUATIONCREATIONCOMMANDS WITH RECIPE NULL");
            if (withStacksInStorage == null)
                throw new ApplicationException("WITHSTACKSINSTORAGE SHOULD NEVER BE NULL");

            //if new situation is beginning with an existing verb: do not action the creation.
            //oh: I could have an scc property which is a MUST CREATE override

            var registeredSituations = GetRegisteredSituations();
            var existingSituation = registeredSituations.Find(s => !s.Verb.AllowMultipleInstances && s.Verb.Id == scc.Recipe.ActionId);

            //grabbing existingtoken: just in case some day I want to, e.g., add additional tokens to an ongoing one rather than silently fail the attempt.
            if (existingSituation != null)
            {
                if (existingSituation.State == SituationState.Complete && existingSituation.Verb.Transient)
                {
                    //verb exists already, but it's completed. We don't want to block new temp verbs executing if the old one is complete, because
                    //otherwise there's an exploit to, e.g., leave hazard finished but unresolved to block new ones appearing.
                    //So nothing happens in this branch except logging.
                    NoonUtility.Log("Created duplicate verb, because previous one is both transient and complete.");
                }
                else
                {
                    NoonUtility.Log("Tried to create " + scc.Recipe.Id + " for verb " + scc.Recipe.ActionId + " but that verb is already active.");
                    //end execution here
                    return;
                }
            }

            var builder = Registry.Get<SituationBuilder>();
            var situation =builder.CreateSituation(scc);

            situation.ExecuteHeartbeat(0f);


            //if there's been (for instance) an expulsion, we now want to add the relevant stacks to this situation
            if (withStacksInStorage.Any())
                situation.AcceptTokens(SphereCategory.SituationStorage, withStacksInStorage);


        }

        //public IEnumerable<Token> GetAnimatables()
        //{
        //    var situationTokens = GetRegisteredSituations().Select(s => s.situationAnchor as Token);

        //    return situationTokens.Where(s => s.CanAnimateArt());

    }
}
