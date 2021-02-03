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
        public NullRecipe(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
            //do nothing, we're null
        }


        public static NullRecipe Create()
        {
            return Create(NullVerb.Create());
        }

        
        public static NullRecipe Create(Verb forVerb)
        {
            Hashtable nullht = new Hashtable();
            EntityData fauxEntityData = new EntityData("nullrecipe", nullht);
            var nr = new NullRecipe(fauxEntityData, new ContentImportLog());
            nr.Craftable = false;
            nr.HintOnly = true;
    

            nr.ActionId = forVerb.Id;
            nr.Label = forVerb.Label;
            nr.Description = forVerb.Description;
         
            return nr;
        }

        public override int Priority => -1;
    }
}