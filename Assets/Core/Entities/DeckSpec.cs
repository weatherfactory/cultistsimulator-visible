using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using Noon;
using UnityEngine.SocialPlatforms;

namespace Assets.Core.Entities
{
    public interface IDeckSpec
    {
        /// <summary>
        /// resets deckSpec (with up to date version of each stack). Use this when first creating the deckSpec
        /// </summary>

        string Id { get; }
        List<string> Spec { get; set; }
        string DefaultCard { get; set; }
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

    [FucineImportable("decks")]
 public class DeckSpec : AbstractEntity<DeckSpec>, IDeckSpec,IEntityWithId
    {

        [FucineValue("")]
        public string DefaultCard { get; set; }

        [FucineValue(false)]
        public bool ResetOnExhaustion { get; set; }

        [FucineValue(DefaultValue = "", Localise = true)]
        public string Label { get; set; }

        [FucineValue(DefaultValue = "", Localise = true)]
        public string Description { get; set; }

        [FucineValue("")]
      public string Comments { get; set; }

        /// <summary>
        /// This is used for internal decks only - default is 1. It allows us to specify >1 draw for an internal deck's default deckeffect.
        /// </summary>
        [FucineValue(1)]
        public int Draws { get; set; }


        //Spec determines which cards start in the deckSpec after each reset
        [FucineList(ValidateAsElementId = true)]
        public List<string> Spec
        {
            get => _spec;
            set => _spec = value;
        }

        [FucineDict(KeyMustExistIn = "Spec",Localise=true)]
        public Dictionary<string, string> DrawMessages
        {
            get => _drawMessages;
            set => _drawMessages = value;
        }

        [FucineDict]
        public Dictionary<string, string> DefaultDrawMessages
        {
            get => _defaultDrawMessages;
            set => _defaultDrawMessages = value;
        }


        //----------
        private List<string> _spec = new List<string>();
        private readonly Dictionary<string, List<string>> _uniquenessGroupsWithCards = new Dictionary<string, List<string>>();
        private Dictionary<string, string> _defaultDrawMessages = new Dictionary<string, string>();
        private Dictionary<string, string> _drawMessages = new Dictionary<string, string>();
      

        public DeckSpec(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
            RegisterUniquenessGroups(populatedCompendium);

        }


        public void RegisterUniquenessGroups(ICompendium compendium)
        {
            if(!Spec.Any())
                throw new NotImplementedException("We're trying to register uniqueness groups for a DeckSpec before populating it with cards.");

            foreach (var c in Spec)
            { 
                var e = compendium.GetEntityById<Element>(c);
                if (e == null)
                {
                    throw new ApplicationException("Can't find element '" + c + " from deck id " + Id + "'");
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

        public DeckSpec()
        {}



    }

   
}
