﻿using System;
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
        void RemoveAllCardsWithId(string elementId);
        Dictionary<string, string> GetDefaultDrawMessages();
        Dictionary<string, string> GetDrawMessages();
    }


 public class DeckSpec : IDeckSpec
    {
        private string _id;
        //DeckSpec startingCards determines which cards start in the deckSpec after each reset
        public List<string> StartingCards { get; set; }
        public string DefaultCardId { get; set; }
        public bool ResetOnExhaustion { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public Dictionary<string,string> DrawMessages { get; set; }
        public Dictionary<string, string> DefaultDrawMessages { get; set; }

        public DeckSpec(string id,List<string> startingCards,string defaultCardId,bool resetOnExhaustion)
        {
            _id = id;
            StartingCards = startingCards;
            DefaultCardId = defaultCardId;
            ResetOnExhaustion = resetOnExhaustion;
            DrawMessages=new Dictionary<string, string>();
            DefaultDrawMessages=new Dictionary<string, string>();
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
