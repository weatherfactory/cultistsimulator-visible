using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using UnityEngine.UIElements;

namespace SecretHistories.Entities
{
    public class LinkedRecipeDetails : AbstractEntity<LinkedRecipeDetails>
    {


        [FucineValue(100)]
        public int Chance { get; set; }

        [FucineValue(false)]
        public bool Additional { get; set; }

        [FucineValue]
        public string ToPath { get; set; }

        /// <summary>
        /// Specify a challenge based on aspect quality, as either base or advanced. If there's more than one challenge,
        /// the most generous % chance will be used.
        /// </summary>
        [FucineDict]
        public Dictionary<string, string> Challenges { get; set; }

        [FucineSubEntity(typeof(Expulsion))]
        public Expulsion Expulsion { get; set; }

        public bool ShouldAlwaysSucceed()
        {
            return Chance >= 100 && Challenges.Count <= 0;
        }



        public static  LinkedRecipeDetails AsCurrentRecipe(Recipe r)
        {
            var l=new LinkedRecipeDetails(r.Id);
            l.Chance = 0;
            l.Additional = false;
            l.Challenges=new Dictionary<string, string>();
            l.ToPath = FucinePath.Current().ToString();
            return l;
        }

        private LinkedRecipeDetails(string recipeId)
        {
            SetId(recipeId);
        }

        public LinkedRecipeDetails(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
        }

        


        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            Hashtable unknownProperties = PopAllUnknownProperties();
           var entityData=new EntityData(unknownProperties);
            if (unknownProperties.Keys.Count > 0)
            {
                //unknown properties in a LinkedRecipeDetails are probably an internal recipe
                unknownProperties.Add("id", Id); //the LinkedRecipeDetails will already have absorbed the recipe ID

                Recipe internalRecipe = new Recipe(entityData, log);

                populatedCompendium.TryAddEntity(internalRecipe);

                internalRecipe.OnPostImport(log, populatedCompendium); //this will log any issues with the import
            }
        }
    }
}