using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Noon;
using NUnit.Framework;

namespace CS.Tests
{
    [TestFixture]
    public class Integration_ImportTests
    {
        //these are JSON integration tests
        private const string RECIPE_1_ID = "foo";
        private const string RECIPE_1_LABEL = "foo bar faz boz fup doo. Doo?";
        private const string RECIPE_1_ACTIONID = "fooify";
        private const string RECIPE_1_CRAFTABLE = "false";
        private const string RECIPE_1_START_DESCRIPTION = "this # is the start!";
        private const string RECIPE_1_DESCRIPTION = "to play / us out...";
        private const string RECIPE_1_WARMUP = "6";
        private const string RECIPE_1_LOOP = "someid";
        private const string RECIPE_1_ENDING = "anending";
        private const string ASPECT_1_ID = "aspect1id";
        private const int ASPECT_1_VALUE = 2;
        private const string ASPECT_2_ID = "aspect2id";
        private const int ASPECT_2_VALUE = -4;
        private const string PI_ASPECT_1_ID = "persistedaspect1";
        private const int PI_ASPECT_1_VALUE = 1;
        private const string PI_ASPECT_2_ID = "persistedaspect2";
        private const int PI_ASPECT_2_VALUE = 2;
        private const string RC_ASPECT_1_ID = "retrievedaspect1";
        private const int RC_ASPECT_1_VALUE = 1;
        private const string RC_ASPECT_2_ID = "retrievedaspect2";
        private const int RC_ASPECT_2_VALUE = 2;
        private const string EFFECT_1_ID = "effect1id";
        private const int EFFECT_1_VALUE = 9;
        private const string EFFECT_2_ID = "effect2id";
        private const int EFFECT_2_VALUE = -19;
        private const string ALTERNATIVE_1_ID = "alternative1";
        private const string ALTERNATIVE_2_ID = "alternative2";
        private const int ALTERNATIVE_1_CHANCE = 10;
        private const int ALTERNATIVE_2_CHANCE = 100;
        private const string SLOT_LABEL = "slotId";
        private const string SLOT_REQUIRED_ASPECT_ID = "slotaspect1";
        private const int SLOT_REQUIRED_ASPECT_VALUE = 1;
        private const string SLOT_FORBIDDEN_ASPECT_ID = "slotaspect2";
        private const int SLOT_FORBIDDEN_ASPECT_VALUE = 1;




        [Test]
        public void RecipesImportFromHashtable()
        {
            ContentImporter cm = new ContentImporter();
            ArrayList recipesToImport = new ArrayList();
            Hashtable htRecipe = new Hashtable();
            Hashtable htEffects = new Hashtable();
            Hashtable htRequirements = new Hashtable();
            Hashtable htPersistIngredients = new Hashtable();

            Hashtable htSlotSpecifications = new Hashtable()
        { { Constants.KREQUIRED,new Hashtable()
        {
            {SLOT_REQUIRED_ASPECT_ID,SLOT_REQUIRED_ASPECT_VALUE}
        }},
        { Constants.KFORBIDDEN,new Hashtable()
        {
            {SLOT_FORBIDDEN_ASPECT_ID,SLOT_FORBIDDEN_ASPECT_VALUE}
        }},
                {Constants.KGREEDY,"true"}
            };
            Hashtable htSlotOuterTable = new Hashtable() { { SLOT_LABEL, htSlotSpecifications } };

            Hashtable htRetrievesContents = new Hashtable();
            ArrayList alAlternatives = new ArrayList();

            htRequirements.Add(ASPECT_1_ID, ASPECT_1_VALUE);
            htRequirements.Add(ASPECT_2_ID, ASPECT_2_VALUE);

            htEffects.Add(EFFECT_1_ID, EFFECT_1_VALUE);
            htEffects.Add(EFFECT_2_ID, EFFECT_2_VALUE);

            htPersistIngredients.Add(PI_ASPECT_1_ID, PI_ASPECT_1_VALUE);
            htPersistIngredients.Add(PI_ASPECT_2_ID, PI_ASPECT_2_VALUE);

            htRetrievesContents.Add(RC_ASPECT_1_ID, RC_ASPECT_1_VALUE);
            htRetrievesContents.Add(RC_ASPECT_2_ID, RC_ASPECT_2_VALUE);

            Hashtable alternative1 = new Hashtable()
            {
                {Constants.KID,ALTERNATIVE_1_ID},
                {Constants.KCHANCE,ALTERNATIVE_1_CHANCE }

            };

            Hashtable alternative2 = new Hashtable()
                            {
                {Constants.KID,ALTERNATIVE_2_ID},
                {Constants.KCHANCE,ALTERNATIVE_2_CHANCE },
                {Constants.KADDITIONAL,true }
            };



            alAlternatives.Add(alternative1);
            alAlternatives.Add(alternative2);

            htRecipe.Add(Constants.KID, RECIPE_1_ID);
            htRecipe.Add(Constants.KLABEL, RECIPE_1_LABEL);
            htRecipe.Add(Constants.KACTIONID, RECIPE_1_ACTIONID);
            htRecipe.Add(Constants.KSTARTDESCRIPTION, RECIPE_1_START_DESCRIPTION);
            htRecipe.Add(Constants.KDESCRIPTION, RECIPE_1_DESCRIPTION);
            htRecipe.Add(Constants.KWARMUP, RECIPE_1_WARMUP);
            htRecipe.Add(Constants.KLOOP, RECIPE_1_LOOP);
            htRecipe.Add(Constants.KENDING, RECIPE_1_ENDING);
            htRecipe.Add(Constants.KCRAFTABLE, RECIPE_1_CRAFTABLE);
            htRecipe.Add(Constants.KREQUIREMENTS, htRequirements);
            htRecipe.Add(Constants.KEFFECTS, htEffects);
            htRecipe.Add(Constants.KPERSISTINGREDIENTSWITH, htPersistIngredients);
            htRecipe.Add(Constants.KRETRIEVESCONTENTSWITH, htRetrievesContents);
            htRecipe.Add(Constants.KSLOTS, htSlotOuterTable);

            htRecipe.Add(Constants.KALTERNATIVERECIPES, alAlternatives);


            recipesToImport.Add(htRecipe);
            cm.PopulateRecipeList(recipesToImport);

            List<Recipe> recipesImported = cm.Recipes;

            Assert.AreEqual(1, recipesImported.Count);
            ConfirmRecipeTextImported(recipesImported);

            ConfirmRecipeOtherPropertiesImported(recipesImported);

            ConfirmRecipeRequirementsImported(recipesImported);

            ConfirmRecipeEffectsImported(recipesImported);

            ConfirmRecipePersistedAndRetrievedElements(recipesImported);

            ConfirmRecipeAlternativesImported(recipesImported);

            ConfirmRecipeSlotsImported(recipesImported);



        }

        private void ConfirmRecipeSlotsImported(List<Recipe> recipesImported)
        {
            Recipe r = recipesImported.First();
            Assert.AreEqual(1, r.SlotSpecifications.Count);
            Assert.AreEqual(SLOT_LABEL, r.SlotSpecifications.Single().Label);
            Assert.IsTrue(r.SlotSpecifications.Single().Greedy);
            Assert.AreEqual(SLOT_REQUIRED_ASPECT_ID, r.SlotSpecifications.Single().Required.Single().Key);
            Assert.AreEqual(SLOT_REQUIRED_ASPECT_VALUE, r.SlotSpecifications.Single().Required.Single().Value);
            Assert.AreEqual(SLOT_FORBIDDEN_ASPECT_ID, r.SlotSpecifications.Single().Forbidden.Single().Key);
            Assert.AreEqual(SLOT_FORBIDDEN_ASPECT_VALUE, r.SlotSpecifications.Single().Forbidden.Single().Value);
        }

        private void ConfirmRecipePersistedAndRetrievedElements(List<Recipe> recipesImported)
        {
            Assert.AreEqual(PI_ASPECT_1_VALUE, recipesImported.First().PersistsIngredientsWith[PI_ASPECT_1_ID]);
            Assert.AreEqual(PI_ASPECT_2_VALUE, recipesImported.First().PersistsIngredientsWith[PI_ASPECT_2_ID]);

            Assert.AreEqual(RC_ASPECT_1_VALUE, recipesImported.First().RetrievesContentsWith[RC_ASPECT_1_ID]);
            Assert.AreEqual(RC_ASPECT_2_VALUE, recipesImported.First().RetrievesContentsWith[RC_ASPECT_2_ID]);
        }

        private static void ConfirmRecipeAlternativesImported(List<Recipe> recipesImported)
        {
            Assert.AreEqual(ALTERNATIVE_1_ID, recipesImported.First().AlternativeRecipes[0].Id);
            Assert.AreEqual(ALTERNATIVE_1_CHANCE, recipesImported.First().AlternativeRecipes[0].Chance);
            Assert.IsFalse(recipesImported.First().AlternativeRecipes[0].Additional);

            Assert.AreEqual(ALTERNATIVE_2_ID, recipesImported.First().AlternativeRecipes[1].Id);
            Assert.AreEqual(ALTERNATIVE_2_CHANCE, recipesImported.First().AlternativeRecipes[1].Chance);
            Assert.IsTrue(recipesImported.First().AlternativeRecipes[1].Additional);

        }

        private static void ConfirmRecipeEffectsImported(List<Recipe> recipesImported)
        {
            Assert.AreEqual(EFFECT_1_VALUE, recipesImported.First().Effects[EFFECT_1_ID]);
            Assert.AreEqual(EFFECT_2_VALUE, recipesImported.First().Effects[EFFECT_2_ID]);
        }

        private static void ConfirmRecipeRequirementsImported(List<Recipe> recipesImported)
        {
            Assert.AreEqual(ASPECT_1_VALUE, recipesImported.First().Requirements[ASPECT_1_ID]);
            Assert.AreEqual(ASPECT_2_VALUE, recipesImported.First().Requirements[ASPECT_2_ID]);
        }

        private static void ConfirmRecipeOtherPropertiesImported(List<Recipe> recipesImported)
        {
            Assert.AreEqual(Convert.ToInt32(RECIPE_1_WARMUP), recipesImported.First().Warmup);
            Assert.AreEqual(RECIPE_1_ACTIONID, recipesImported.First().ActionId);
            Assert.AreEqual(Convert.ToBoolean(RECIPE_1_CRAFTABLE), recipesImported.First().Craftable);
            Assert.AreEqual(RECIPE_1_LOOP, recipesImported.First().Loop);
            Assert.AreEqual(RECIPE_1_ENDING, recipesImported.First().Ending);
        }

        private static void ConfirmRecipeTextImported(List<Recipe> recipesImported)
        {
            Assert.AreEqual(RECIPE_1_ID, recipesImported.First().Id);
            Assert.AreEqual(RECIPE_1_LABEL, recipesImported.First().Label);
            Assert.AreEqual(RECIPE_1_START_DESCRIPTION, recipesImported.First().StartDescription);
            Assert.AreEqual(RECIPE_1_DESCRIPTION, recipesImported.First().Description);
        }
    }
}
