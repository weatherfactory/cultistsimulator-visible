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
        private const string RECIPE_1_LINKED = "someid";
        private const string RECIPE_1_ENDING = "anending";
        private const int RECIPE_MAX_EXECUTIONS = 3;
        private const string RECIPE_BURN_IMAGE = "shadows_and_stains";
   
        private const string REQ_1_ID = "req1id";
        private const int REQ_1_VALUE = 2;
        private const string REQ_2_ID = "req2id";
        private const int REQ_2_VALUE = -4;
        private const string EFFECT_1_ID = "effect1id";
        private const int EFFECT_1_VALUE = 9;
        private const string EFFECT_2_ID = "effect2id";
        private const int EFFECT_2_VALUE = -19;
        private const string ASPECT_ID = "aspectforxtriggerid";
        private const int ASPECT_VALUE = 1;
        private const string ALTERNATIVE_1_ID = "alternative1";
        private const string ALTERNATIVE_2_ID = "alternative2";
        private const int ALTERNATIVE_1_CHANCE = 10;
        private const int ALTERNATIVE_2_CHANCE = 100;
        private const string LINKED_1_ID = "linked1";
        private const string LINKED_2_ID = "linked2";
        private const int LINKED_1_CHANCE = 20;
        private const int LINKED_2_CHANCE = 90;
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
            Hashtable htAspects=new Hashtable();

            Hashtable htSlotSpecifications = new Hashtable()
        { { NoonConstants.KREQUIRED,new Hashtable()
        {
            {SLOT_REQUIRED_ASPECT_ID,SLOT_REQUIRED_ASPECT_VALUE}
        }},
        { NoonConstants.KFORBIDDEN,new Hashtable()
        {
            {SLOT_FORBIDDEN_ASPECT_ID,SLOT_FORBIDDEN_ASPECT_VALUE}
        }},
                {NoonConstants.KGREEDY,"true"},
                {NoonConstants.KCONSUMES,"true"},
            };
            Hashtable htSlotOuterTable = new Hashtable() { { SLOT_LABEL, htSlotSpecifications } };

       
            
            htRequirements.Add(REQ_1_ID, REQ_1_VALUE.ToString());
            htRequirements.Add(REQ_2_ID, REQ_2_VALUE.ToString());

            htEffects.Add(EFFECT_1_ID, EFFECT_1_VALUE.ToString());
            htEffects.Add(EFFECT_2_ID, EFFECT_2_VALUE.ToString());

            htAspects.Add(ASPECT_ID,ASPECT_VALUE.ToString());

            ArrayList alAlternatives = new ArrayList();

            Hashtable alternative1 = new Hashtable()
            {
                {NoonConstants.KID,ALTERNATIVE_1_ID},
                {NoonConstants.KCHANCE,ALTERNATIVE_1_CHANCE.ToString() }

            };

            Hashtable alternative2 = new Hashtable()
                            {
                {NoonConstants.KID,ALTERNATIVE_2_ID},
                {NoonConstants.KCHANCE,ALTERNATIVE_2_CHANCE.ToString() },
                {NoonConstants.KADDITIONAL,true }
            };
        
            alAlternatives.Add(alternative1);
            alAlternatives.Add(alternative2);


            ArrayList allinkeds = new ArrayList();

            Hashtable linked1 = new Hashtable()
            {
                {NoonConstants.KID,LINKED_1_ID},
                {NoonConstants.KCHANCE,LINKED_1_CHANCE.ToString() }

            };

            Hashtable linked2 = new Hashtable()
            {
                {NoonConstants.KID,LINKED_2_ID},
                {NoonConstants.KCHANCE,LINKED_2_CHANCE.ToString() },
                {NoonConstants.KADDITIONAL,true }
            };

            allinkeds.Add(linked1);
            allinkeds.Add(linked2);


            htRecipe.Add(NoonConstants.KID, RECIPE_1_ID);
            htRecipe.Add(NoonConstants.KLABEL, RECIPE_1_LABEL);
            htRecipe.Add(NoonConstants.KACTIONID, RECIPE_1_ACTIONID);
            htRecipe.Add(NoonConstants.KSTARTDESCRIPTION, RECIPE_1_START_DESCRIPTION);
            htRecipe.Add(NoonConstants.KDESCRIPTION, RECIPE_1_DESCRIPTION);
            htRecipe.Add(NoonConstants.KWARMUP, RECIPE_1_WARMUP);
            htRecipe.Add(NoonConstants.KENDING, RECIPE_1_ENDING);
            htRecipe.Add(NoonConstants.KMAXEXECUTIONS,RECIPE_MAX_EXECUTIONS.ToString());
            htRecipe.Add(NoonConstants.KBURNIMAGE,RECIPE_BURN_IMAGE.ToString());
            htRecipe.Add(NoonConstants.KCRAFTABLE, RECIPE_1_CRAFTABLE);
            htRecipe.Add(NoonConstants.KREQUIREMENTS, htRequirements);
            htRecipe.Add(NoonConstants.KEFFECTS, htEffects);
            htRecipe.Add(NoonConstants.KASPECTS,htAspects);
            htRecipe.Add(NoonConstants.KSLOTS, htSlotOuterTable);

            htRecipe.Add(NoonConstants.KALTERNATIVERECIPES, alAlternatives);

            recipesToImport.Add(htRecipe);
            cm.PopulateRecipeList(recipesToImport);

            List<Recipe> recipesImported = cm.Recipes;

            Assert.AreEqual(1, recipesImported.Count);
            ConfirmRecipeTextImported(recipesImported);

            ConfirmRecipeOtherPropertiesImported(recipesImported);

            ConfirmRecipeRequirementsImported(recipesImported);

            ConfirmRecipeEffectsImported(recipesImported);

            ConfirmRecipeAspectsImported(recipesImported);


            ConfirmRecipeAlternativesImported(recipesImported);
            ConfirmLinkedRecipesImported(recipesImported);

            ConfirmRecipeSlotsImported(recipesImported);



        }



        private void ConfirmRecipeSlotsImported(List<Recipe> recipesImported)
        {
            Recipe r = recipesImported.First();
            Assert.AreEqual(1, r.SlotSpecifications.Count);
            Assert.AreEqual(SLOT_LABEL, r.SlotSpecifications.Single().Id);
            Assert.IsTrue(r.SlotSpecifications.Single().Greedy);
            Assert.IsTrue(r.SlotSpecifications.Single().Consumes);
            Assert.AreEqual(SLOT_REQUIRED_ASPECT_ID, r.SlotSpecifications.Single().Required.Single().Key);
            Assert.AreEqual(SLOT_REQUIRED_ASPECT_VALUE, r.SlotSpecifications.Single().Required.Single().Value);
            Assert.AreEqual(SLOT_FORBIDDEN_ASPECT_ID, r.SlotSpecifications.Single().Forbidden.Single().Key);
            Assert.AreEqual(SLOT_FORBIDDEN_ASPECT_VALUE, r.SlotSpecifications.Single().Forbidden.Single().Value);
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

        private static void ConfirmLinkedRecipesImported(List<Recipe> recipesImported)
        {
            Assert.AreEqual(LINKED_1_ID, recipesImported.First().LinkedRecipes[0].Id);
            Assert.AreEqual(LINKED_1_CHANCE, recipesImported.First().LinkedRecipes[0].Chance);
            Assert.IsFalse(recipesImported.First().LinkedRecipes[0].Additional);

            Assert.AreEqual(LINKED_2_ID, recipesImported.First().LinkedRecipes[1].Id);
            Assert.AreEqual(LINKED_2_CHANCE, recipesImported.First().LinkedRecipes[1].Chance);
            Assert.IsTrue(recipesImported.First().LinkedRecipes[1].Additional);

        }

        private void ConfirmRecipeAspectsImported(List<Recipe> recipesImported)
        {
            Assert.AreEqual(ASPECT_VALUE,recipesImported.First().Aspects[ASPECT_ID]);
        }

        private static void ConfirmRecipeEffectsImported(List<Recipe> recipesImported)
        {
            Assert.AreEqual(EFFECT_1_VALUE, recipesImported.First().Effects[EFFECT_1_ID]);
            Assert.AreEqual(EFFECT_2_VALUE, recipesImported.First().Effects[EFFECT_2_ID]);
        }

        private static void ConfirmRecipeRequirementsImported(List<Recipe> recipesImported)
        {
            Assert.AreEqual(REQ_1_VALUE, recipesImported.First().Requirements[REQ_1_ID]);
            Assert.AreEqual(REQ_2_VALUE, recipesImported.First().Requirements[REQ_2_ID]);
        }

        private static void ConfirmRecipeOtherPropertiesImported(List<Recipe> recipesImported)
        {
            Assert.AreEqual(Convert.ToInt32(RECIPE_1_WARMUP), recipesImported.First().Warmup);
            Assert.AreEqual(RECIPE_1_ACTIONID, recipesImported.First().ActionId);
            Assert.AreEqual(Convert.ToBoolean(RECIPE_1_CRAFTABLE), recipesImported.First().Craftable);
            Assert.AreEqual(RECIPE_1_ENDING, recipesImported.First().EndingFlag);
            Assert.AreEqual(RECIPE_MAX_EXECUTIONS,recipesImported.First().MaxExecutions);
            Assert.AreEqual(RECIPE_BURN_IMAGE,recipesImported.First().BurnImage);
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
