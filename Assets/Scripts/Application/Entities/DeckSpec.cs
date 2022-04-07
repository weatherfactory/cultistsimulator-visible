using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using UnityEngine.SocialPlatforms;

namespace SecretHistories.Entities
{


    [FucineImportable("decks")]
 public class DeckSpec : AbstractEntity<DeckSpec>
    {

        [FucineValue("")]
        public string DefaultCard { get; set; }

        [FucineValue(false)]
        public bool ResetOnExhaustion { get; set; }

        [FucineValue(DefaultValue = "", Localise = true)]
        public string Label { get; set; }

        [FucineValue(DefaultValue = "", Localise = true)]
        public string Description { get; set; }

        [FucineValue(DefaultValue = "books")]
        public string Cover { get; set; }

        [FucineValue("")]
      public string Comments { get; set; }


      /// <summary>
      /// if set, only appears when the character begins with that legacy
      /// </summary>
      [FucineValue("")] public string ForLegacyFamily { get; set; }

        /// <summary>
        /// This is used for internal decks only - default is 1. It allows us to specify >1 draw for an internal deck's default deckeffect.
        /// </summary>
        [FucineValue(1)]
        public int Draws { get; set; }


        //DeckSpec determines which cards start in the deckSpec after each reset
        [FucineList(ValidateAsElementId = true)]
        public List<string> Spec
        {
            get => _spec;
            set => _spec = value;
        }

        [FucineDict(KeyMustExistIn = "DeckSpec",Localise=true)]
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

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            RegisterUniquenessGroups(populatedCompendium);
        }


        public void RegisterUniquenessGroups(Compendium compendium)
        {
            if(!Spec.Any())
                throw new NotImplementedException("We're trying to register uniqueness groups for a DeckSpec before populating it with cards.");

            foreach (var c in Spec)
            { 
                var e = compendium.GetEntityById<Element>(c);
                if (e == null)
                {
                    NoonUtility.Log("Can't find element '" + c + " in spec for deck id " + Id + "'",2,VerbosityLevel.Essential);
                    continue;
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
