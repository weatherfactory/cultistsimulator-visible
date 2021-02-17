using System;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Commands;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using SecretHistories.UI;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Services;


namespace SecretHistories.Entities
{

    [Immanence(typeof(SituationsCatalogue))]
    public class SituationsCatalogue
    {
        private  List<Situation> _currentSituations;
        

        public SituationsCatalogue()
        {
           Reset();
            
        }
        
        public void Reset()
        {
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

        public Situation GetSituationByPath(FucinePath path)
        {
            try
            {
                return _currentSituations.DefaultIfEmpty(NullSituation.Create()).SingleOrDefault(s => s.Path == path);

            }
            catch (InvalidOperationException)
            {
                NoonUtility.LogWarning("More than one situation with path " + path + "  - returning the first.");
                return _currentSituations.First(s => s.Path == path);
            }
        }



        public void HaltSituation(string toHaltId, int maxToHalt)
        {
            var situationsCatalogue = Watchman.Get<SituationsCatalogue>();
            int i = 0;
            //Halt the verb if the actionId matches BEARING IN MIND WILDCARD

            if (toHaltId.Contains('*'))
            {
                string wildcardToDelete = toHaltId.Remove(toHaltId.IndexOf('*'));

                foreach (var s in situationsCatalogue.GetRegisteredSituations())
                {
                    if (s.Verb.Id.StartsWith(wildcardToDelete))
                    {
                        s.CommandQueue.AddCommand(new TryHaltSituationCommand());
                        s.ExecuteHeartbeat(0f);
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
                        s.CommandQueue.AddCommand(new TryHaltSituationCommand());
                        s.ExecuteHeartbeat(0f);
                        i++;
                    }
                    if (i >= maxToHalt)
                        break;
                }
            }
        }

        public void DeleteSituation(string toDeleteId, int maxToDelete)
        {
            var situationsCatalogue = Watchman.Get<SituationsCatalogue>();
            int i = 0;
            //Delete the verb if the actionId matches BEARING IN MIND WILDCARD

            if (toDeleteId.Contains('*'))
            {
                string wildcardToDelete = toDeleteId.Remove(toDeleteId.IndexOf('*'));

                foreach (var s in situationsCatalogue.GetRegisteredSituations())
                {
                    if (s.Verb.Id.StartsWith(wildcardToDelete))
                    {
                        s.Retire(RetirementVFX.VerbAnchorVanish);
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
                        s.Retire(RetirementVFX.VerbAnchorVanish);
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
