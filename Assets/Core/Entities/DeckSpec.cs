using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Noon;

namespace Assets.Core.Entities
{
    public interface IDeckSpec
    {
        /// <summary>
        /// resets deckSpec (with up to date version of each stack). Use this when first creating the deckSpec
        /// </summary>

        string Id { get; }
        List<string> StartingCards { get; set; }
        string DefaultCardId { get; set; }
        bool ResetOnExhaustion { get; set; }
        string Label { get; set; }
        string Description { get; set; }
        Dictionary<string, string> DrawMessages { get; set; }
        Dictionary<string, string> DefaultDrawMessages { get; set; }
        void RegisterUniquenessGroups(ICompendium compendium);
        List<string> CardsInUniquenessGroup(string uniquenessGroupId);
    }


    public interface IDeckInstance:ISaveable
    {
        /// <summary>
        /// resets deckSpec (with up to date version of each stack). Use this when first creating the deckSpec
        /// </summary>
        void Reset();

        string Id { get; }
        string Draw();
        void Add(string elementId);
        List<string> GetCurrentCardsAsList();
        void EliminateCardWithId(string elementId);
        Dictionary<string, string> GetDefaultDrawMessages();
        Dictionary<string, string> GetDrawMessages();
        void TryAddToEliminatedCardsList(string elementId);
        void EliminateCardsInUniquenessGroup(string elementUniquenessGroup);
    }


 public class DeckSpec : IDeckSpec
    {
        private string _id;
        //DeckSpec startingCards determines which cards start in the deckSpec after each reset
        public List<string> StartingCards { get; set; }
        public string DefaultCardId { get; set; }
        public bool ResetOnExhaustion { get; set; }

     /// <summary>
     /// This is used for internal decks only - default is 1. It allows us to specify >1 draw for an internal deck's default deckeffect.
     /// </summary>
        public int DefaultDraws { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public Dictionary<string,string> DrawMessages { get; set; }
        public Dictionary<string, string> DefaultDrawMessages { get; set; }
        private Dictionary<string, List<string>> _uniquenessGroupsWithCards;

        public DeckSpec(string id, List<string> startingCards, string defaultCardId, bool resetOnExhaustion)
        {
            _id = id;
            StartingCards = startingCards;
            DefaultCardId = defaultCardId;
            ResetOnExhaustion = resetOnExhaustion;
            DrawMessages = new Dictionary<string, string>();
            DefaultDrawMessages = new Dictionary<string, string>();
            _uniquenessGroupsWithCards=new Dictionary<string, List<string>>();
            DefaultDraws = 1;
        }



        public void RegisterUniquenessGroups(ICompendium compendium)
        {
            if(!StartingCards.Any())
                throw new NotImplementedException("We're trying to register uniqueness groups for a DeckSpec before populating it with cards.");

            foreach (var c in StartingCards)
            { 
                var e = compendium.GetElementById(c);
                if (e == null)
                {
                    throw new ApplicationException("Can't find element '" + c + " from deck id " + _id + "'");
                }

                if (!string.IsNullOrEmpty(e.UniquenessGroup))
                {
                    if (!_uniquenessGroupsWithCards.ContainsKey(e.UniquenessGroup))
                    {
                        _uniquenessGroupsWithCards.Add(e.UniquenessGroup,new List<string>());
                    }

                    if(!_uniquenessGroupsWithCards[e.UniquenessGroup].Contains(c))
                        _uniquenessGroupsWithCards[e.UniquenessGroup].Add(c);
                }
            }
        }

        public List<string> CardsInUniquenessGroup(string uniquenessGroupId)
        {
            if (_uniquenessGroupsWithCards.ContainsKey(uniquenessGroupId))

                return _uniquenessGroupsWithCards[uniquenessGroupId];
            else
                return null;


        
        }


        


        /// <summary>
        /// resets deckSpec (with up to date version of each stack). Use this when first creating the deckSpec
        /// </summary>

        public string Id
        {
            get { return _id; }
        }





    }

   
}
