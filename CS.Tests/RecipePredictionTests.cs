using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Noon;
using NSubstitute;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
    public class RecipePredictionTests
    {
        private ICompendium compendium;
        private IDice mockDice;
        private Recipe primaryRecipe;
        private Recipe secondaryRecipe;
        private Recipe tertiaryRecipe;
        private Recipe quaternaryRecipe;

        private const string ACTIONID = "foo";

        [SetUp]
        public void Setup()
        {
            NoonUtility.UnitTestingMode = true;

            primaryRecipe = new Recipe
            {
                Id = "primaryrecipe",
                Description = "A primary sort of description.",
                ActionId = ACTIONID,
                Craftable = true
            };
            secondaryRecipe = new Recipe
            {
                Id = "secondaryrecipe",
                Description = "A more secondary description.",
                ActionId = ACTIONID,
                Craftable = true
            };

            tertiaryRecipe = new Recipe
            {
                Id = "tertiaryrecipe",
                Description = "And a tertiary description.",
                ActionId = ACTIONID,
                Craftable = true
            }; 

            quaternaryRecipe = new Recipe
            {
                Id = "quaternaryrecipe",
                Description = "And still they come (quaternary)",
                ActionId = ACTIONID,
                Craftable = true
            }; 
            

            List<Recipe> allRecipes = new List<Recipe>() { primaryRecipe, secondaryRecipe, tertiaryRecipe, quaternaryRecipe };
            compendium = new Compendium();
            compendium.UpdateRecipes(allRecipes);
            mockDice = Substitute.For<IDice>();
        }
        [Test]
        public void RecipePrediction_ExcludesRecipeWithAspectsUnsatisfied()
        {
            AspectsDictionary aspects = new AspectsDictionary {{"e", 1}};
            primaryRecipe.Requirements.Add("e",2);

            var resultRecipe=compendium.GetFirstRecipeForAspectsWithVerb(aspects, ACTIONID, new Character(), false);
            Assert.AreEqual(secondaryRecipe.Id,resultRecipe.Id);
        }
        [Test]
        public void RecipePrediction_ExcludesRecipeWithNotRequirementUnsatisfied()
        {
            AspectsDictionary aspects = new AspectsDictionary { { "e", 1 } };
            primaryRecipe.Requirements.Add("e", -1); //-1 means 'should have none of this aspect.'

            var resultRecipe = compendium.GetFirstRecipeForAspectsWithVerb(aspects, ACTIONID, new Character(), false);
            Assert.AreEqual(secondaryRecipe.Id, resultRecipe.Id);
        }
    }
    

}

