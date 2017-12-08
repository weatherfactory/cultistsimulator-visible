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
    }


 public class DeckSpec : IDeckSpec
    {
        private string _id;
        //DeckSpec startingCards determines which cards start in the deckSpec after each reset
        public List<string> StartingCards { get; set; }
        public string DefaultCardId { get; set; }

        public DeckSpec(string id,List<string> startingCards,string defaultCardId)
        {
            _id = id;
            StartingCards = startingCards;
            DefaultCardId = defaultCardId;
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
