using System.Collections;
using System.Collections.Generic;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using Assets.Scripts.Entities;

namespace Assets.Core.Entities
{
    public class NullRecipe : Recipe
    {
        public NullRecipe(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
            //do nothing, we're null
        }

        public static NullRecipe Create()
        {
            Hashtable nullht = new Hashtable();
            EntityData fauxEntityData = new EntityData("nullrecipe", nullht);
            var nr= new NullRecipe(fauxEntityData, new ContentImportLog());
            nr.Craftable = false;
            nr.HintOnly = true;
            return nr;
        }

        public static NullRecipe Create(IVerb forVerb)
        {
            var nr = Create();
            nr.ActionId = forVerb.Id;
            nr.Label = forVerb.Label;
            nr.Description = forVerb.Description;
            return nr;
        }

        public override int Priority => -1;
    }
}