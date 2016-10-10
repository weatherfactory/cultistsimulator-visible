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

        [Test]
        public void RecipesImportFromHashtable()
        {
            ContentManager cm=new ContentManager();
            ArrayList recipesToImport=new ArrayList();
            Hashtable htRecipe = new Hashtable();
            htRecipe.Add("id", RECIPE_1_ID);

            recipesToImport.Add(htRecipe);
            RecipeCompendium rc = cm.PopulateRecipeCompendium(recipesToImport);

            List<Recipe> recipesImported = rc.GetAllRecipesAsList();
            Assert.AreEqual(1,recipesImported.Count);
            Assert.AreEqual(RECIPE_1_ID, recipesImported.First().Id);

        }
    }
}
