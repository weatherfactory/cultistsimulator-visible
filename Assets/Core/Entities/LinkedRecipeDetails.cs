﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using UnityEngine.UIElements;

namespace Assets.Core.Entities
{
    public class LinkedRecipeDetails : AbstractEntity, IEntityWithId
    {
        private string _id;

        [FucineId]
        public string Id
        {
            get => _id;
        }

        public void SetId(string id)
        {
            _id = id;
        }


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


        public LinkedRecipeDetails()
        {
        }

        public LinkedRecipeDetails(string id, int chance, bool additional, Expulsion expulsion,
            Dictionary<string, string> Challenges)
        {
            Additional = additional;
            _id = id;
            Chance = chance;
            Expulsion = expulsion;
            this.Challenges = Challenges ?? new Dictionary<string, string>();
        }

        public override void RefineWithCompendium(ContentImportLogger logger, ICompendium populatedCompendium)
        {
            Hashtable unknownProperties = PopAllUnknownProperties();
            if (unknownProperties.Keys.Count > 0)
            {
                //unknown properties in a LinkedRecipeDetails are probably an internal recipe
                unknownProperties.Add("id",Id); //the LinkedRecipeDetails will already have absorbed the recipe ID

                FucinePropertyWalker w = new FucinePropertyWalker(logger, typeof(Recipe));
                Recipe internalRecipe = (Recipe)w.PopulateEntityWith(unknownProperties);

                 populatedCompendium.AddEntity(internalRecipe.Id, typeof(Recipe), internalRecipe);

                 internalRecipe.RefineWithCompendium(logger,populatedCompendium); //this will log any issues with the import
            }
        }
    }
}