using System.Collections;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;

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
            nr.Label = forVerb.Label;
            nr.Description = forVerb.Description;
            return nr;
        }

        public override int Priority => -1;
    }
}