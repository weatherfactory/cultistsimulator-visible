using System;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.Entities;
using SecretHistories.NullObjects;
using SecretHistories.Core;
using SecretHistories.Services;
using UnityEngine.UIElements;

namespace SecretHistories.Entities
{
    public class NullRecipe : Recipe
    {
        private static NullRecipe _instance;
        protected NullRecipe()
        {
            SetId(String.Empty);
            Craftable = false;
            HintOnly = true;
            ActionId = NullVerb.Create().Id;
            Label = String.Empty;
            StartDescription = String.Empty;
            Description = String.Empty;
            DeckEffects=new Dictionary<string, int>();
            Requirements = new Dictionary<string, string>();
            RoomReqs=new Dictionary<string, string>();
            TableReqs=new Dictionary<string, string>();
            ExtantReqs=new Dictionary<string, string>();
            Effects=new Dictionary<string, string>();
            Alt=new List<LinkedRecipeDetails>();
            Linked=new List<LinkedRecipeDetails>();
            Aspects = AspectsDictionary.Empty();
            Mutations = new List<MutationEffect>();
            Purge = new Dictionary<string, int>();
            HaltVerb =new Dictionary<string, int>();
            DeleteVerb=new Dictionary<string, int>();
            Slots=new List<SphereSpec>();
        }

    public static NullRecipe Create()
        {
            if(_instance==null)
                _instance=new NullRecipe();
            return _instance;
        }
        public override bool IsValid()
        {
            return false;
        }

        public override bool RequirementsSatisfiedBy(AspectsInContext aspectsinContext)
        {
            return false;
        }

        public override int Priority => -1;
    }
}