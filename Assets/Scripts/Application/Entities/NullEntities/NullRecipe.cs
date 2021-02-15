using System;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.Interfaces;
using SecretHistories.Entities;
using SecretHistories.NullObjects;
using SecretHistories.Core;

namespace SecretHistories.Entities
{
    public class NullRecipe : Recipe
    {

        protected NullRecipe()
        {
            Craftable = false;
            HintOnly = true;
            ActionId = NullVerb.Create().Id;
            Label = NullVerb.Create().Label;
            StartDescription = NullVerb.Create().Description;
            Description = String.Empty;
            DeckEffects=new Dictionary<string, int>();
            Requirements = new Dictionary<string, string>();
            TableReqs=new Dictionary<string, string>();
            ExtantReqs=new Dictionary<string, string>();
            Effects=new Dictionary<string, string>();
            Aspects = AspectsDictionary.Empty();
            Mutations = new List<MutationEffect>();
            Purge = new Dictionary<string, int>();
            HaltVerb =new Dictionary<string, int>();
            DeleteVerb=new Dictionary<string, int>();
        }

    public static NullRecipe Create()
        {
            return new NullRecipe();
        }
        public override bool IsValid()
        {
            return false;
        }


        public override int Priority => -1;
    }
}