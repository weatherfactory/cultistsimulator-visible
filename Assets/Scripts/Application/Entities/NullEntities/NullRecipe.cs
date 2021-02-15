using System;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.Interfaces;
using SecretHistories.Entities;
using SecretHistories.NullObjects;

namespace SecretHistories.Entities
{
    public class NullRecipe : Recipe
    {

        protected NullRecipe(string actionId, string label, string startDescription): base()
        {
            Craftable = false;
            HintOnly = true;
            ActionId = actionId;
            Label = label;
            StartDescription = startDescription;
            Description = String.Empty;
        }

    public static NullRecipe Create()
        {
            return Create(NullVerb.Create());
        }
        public override bool IsValid()
        {
            return false;
        }

        public static NullRecipe Create(Verb forVerb)
        {
            if(forVerb==null)
                forVerb=NullVerb.Create(); //just in case

            var nr = new NullRecipe(forVerb.Id,forVerb.Label,forVerb.Description);

            return nr;
        }

        public override int Priority => -1;
    }
}