using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using UnityEngine.UIElements;

namespace Assets.Core.Entities
{
    public class LinkedRecipeDetails : AbstractEntity<LinkedRecipeDetails>, IEntityWithId
    {


        [FucineValue(100)]
        public int Chance { get; set; }

        [FucineValue(false)]
        public bool Additional { get; set; }

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


        public LinkedRecipeDetails(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
        }


        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
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