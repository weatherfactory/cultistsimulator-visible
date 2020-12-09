using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Enums;
using Assets.Core.Fucine;
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
            return  new List<Situation>(_currentSituations);
        }


        public void RegisterSituation(Situation situation)
        {
            _currentSituations.Add(situation);
        }

        public void DeregisterSituation(Situation situation)
        {
            _currentSituations.Remove(situation);
        }

        public IEnumerable<Situation> GetSituationsWithVerbOfType(Type verbType)
        {
            return _currentSituations.Where(situation => situation.Verb.GetType() == verbType);
        }

        public IEnumerable<Situation> GetSituationsWithVerbOfActionId(string actionId)
        {
            return _currentSituations.Where(situation => situation.Verb.Id==actionId);
        }


        public Situation GetFirstOpenSituation()
        {
            return GetRegisteredSituations().FirstOrDefault(s => s.IsOpen);
        }

        public Situation GetSituationByPath(SituationPath path)
        {
            try
            {
                return _currentSituations.DefaultIfEmpty(new NullSituation()).SingleOrDefault(s => s.Path == path);

            }
            catch (InvalidOperationException)
            {
                NoonUtility.LogWarning("More than one situation with path " + path + "  - returning the first.");
                return _currentSituations.First(s => s.Path == path);
            }
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
            var situation =builder.CreateSituationWithAnchorAndWindow(scc);

            situation.ExecuteHeartbeat(0f);


            //if there's been (for instance) an expulsion, we now want to add the relevant stacks to this situation
            if (withStacksInStorage.Any())
                situation.AcceptTokens(SphereCategory.SituationStorage, withStacksInStorage);

            return situation;

        }

     

        public void HaltSituation(string toHaltId, int maxToHalt)
        {
            var situationsCatalogue = Registry.Get<SituationsCatalogue>();
            int i = 0;
            //Halt the verb if the actionId matches BEARING IN MIND WILDCARD

            if (toHaltId.Contains('*'))
            {
                string wildcardToDelete = toHaltId.Remove(toHaltId.IndexOf('*'));

                foreach (var s in situationsCatalogue.GetRegisteredSituations())
                {
                    if (s.Verb.Id.StartsWith(wildcardToDelete))
                    {
                        s.Halt();
                        i++;
                    }

                    if (i >= maxToHalt)
                        break;
                }
            }

            else
            {
                foreach (var s in situationsCatalogue.GetRegisteredSituations())
                {
                    if (s.Verb.Id == toHaltId.Trim())
                    {
                        s.Halt();
                        i++;
                    }
                    if (i >= maxToHalt)
                        break;
                }
            }
        }

        public void DeleteSituation(string toDeleteId, int maxToDelete)
        {
            var situationsCatalogue = Registry.Get<SituationsCatalogue>();
            int i = 0;
            //Delete the verb if the actionId matches BEARING IN MIND WILDCARD

            if (toDeleteId.Contains('*'))
            {
                string wildcardToDelete = toDeleteId.Remove(toDeleteId.IndexOf('*'));

                foreach (var s in situationsCatalogue.GetRegisteredSituations())
                {
                    if (s.Verb.Id.StartsWith(wildcardToDelete))
                    {
                        s.Retire();
                        i++;
                    }

                    if (i >= maxToDelete)
                        break;
                }
            }

            else
            {
                foreach (var s in situationsCatalogue.GetRegisteredSituations())
                {
                    if (s.Verb.Id == toDeleteId.Trim())
                    {
                        s.Retire();
                        i++;
                    }
                    if (i >= maxToDelete)
                        break;
                }
            }
        }

        //public IEnumerable<Token> GetAnimatables()
        //{
        //    var situationTokens = GetRegisteredSituations().Select(s => s.situationAnchor as Token);

        //    return situationTokens.Where(s => s.CanAnimateArt());

    }
}
