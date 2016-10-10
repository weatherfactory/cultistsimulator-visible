using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CS.Tests
{
    public class ImportTests
    {
        //these are JSON integration tests
        private const string RECIPE_1_ID = "foo";
        private const string ASPECT_1_ID="aspect1id";
        private const int ASPECT_1_VALUE =2;
        private const string ASPECT_2_ID = "aspect2id";
        private const int ASPECT_2_VALUE = 4;

        [Test]
        public void RecipesImportFromHashtable()
        {
            ContentManager cm=new ContentManager();
            ArrayList recipesToImport=new ArrayList();
            Hashtable htRecipe = new Hashtable();
            Hashtable htRequirements = new Hashtable();
            
            htRequirements.Add(ASPECT_1_ID, ASPECT_1_VALUE);
            htRequirements.Add(ASPECT_2_ID, ASPECT_2_VALUE);

            htRecipe.Add("id", RECIPE_1_ID);
            htRecipe.Add("requirements", htRequirements);

            recipesToImport.Add(htRecipe);
            RecipeCompendium rc = cm.PopulateRecipeCompendium(recipesToImport);

            List<Recipe> recipesImported = rc.GetAllRecipesAsList();
            Assert.AreEqual(1,recipesImported.Count);
            Assert.AreEqual(RECIPE_1_ID, recipesImported.First().Id);
            Assert.AreEqual(ASPECT_1_VALUE, recipesImported.First().Requirements[ASPECT_1_ID]);
            Assert.AreEqual(ASPECT_2_VALUE, recipesImported.First().Requirements[ASPECT_2_ID]);

        }
    }
}
