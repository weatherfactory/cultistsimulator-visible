using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using NSubstitute;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
    public class RecipeConductorTests
    {
        private ICompendium compendium;
        private IDice mockDice;
        private Recipe primaryRecipe;
        private Recipe secondaryRecipe;
        private Recipe tertiaryRecipe;
        private Recipe quaternaryRecipe;
        private RecipeAlternative recipeAlternative;
        private RecipeConductor rc;

        [SetUp]
        public void Setup()
        {
            primaryRecipe = new Recipe
            {
                Id = "primaryrecipe",
                Description = "A primary sort of description.",
            };
            secondaryRecipe = new Recipe
            {
                Id = "secondaryrecipe",
                Description = "A more secondary description."
            };

            tertiaryRecipe = new Recipe
            {
                Id = "tertiaryrecipe",
                Description = "And a tertiary description."
            }; //this isn't always added as an alternative

            quaternaryRecipe = new Recipe
            {
                Id = "quaternaryrecipe",
                Description = "And still they come (quaternary)"
            }; //this isn't always added as an alternative
            
            primaryRecipe.Effects.Add("primaryrecipeeffect", 1);

            recipeAlternative = new RecipeAlternative(secondaryRecipe.Id, 50, false);
            primaryRecipe.AlternativeRecipes.Add(recipeAlternative);


            List<Recipe> allRecipes = new List<Recipe>() { primaryRecipe, secondaryRecipe, tertiaryRecipe, quaternaryRecipe };
            compendium = new Compendium();
            compendium.UpdateRecipes(allRecipes);
            mockDice = Substitute.For<IDice>();
        }
        [Test]
        public void RecipeConductor_SuppliesLoopRecipeToCompletedSituation()
        {
            primaryRecipe.Loop = primaryRecipe.Id;

            rc = new RecipeConductor(compendium, null,mockDice);
            var loopedRecipe = rc.GetLoopedRecipe(primaryRecipe);

            Assert.AreEqual(primaryRecipe,loopedRecipe);
        }

        [Test]
        public void RecipeConductor_SuppliesNullLoopedRecipe_WhenRecipeConditionsNotSatisfied()
        {
            primaryRecipe.Loop = primaryRecipe.Id;
            primaryRecipe.Requirements.Add("loopedRecipeReq",2);
            rc=new RecipeConductor(compendium,new AspectsDictionary() { {"loopedRecipeReq",1}}, mockDice);
            var loopedRecipe = rc.GetLoopedRecipe(primaryRecipe);
            Assert.IsNull(loopedRecipe);
        }

        [Test]
        public void AlternateRecipeExecutes_IfNoRequirements_AndDiceRollSatisfied()
        {
            mockDice.Rolld100().Returns(recipeAlternative.Chance);

     rc = new RecipeConductor(compendium, null, mockDice);
            IEnumerable<Recipe> recipesToExecute = rc.GetActualRecipesToExecute(primaryRecipe);

            Assert.AreEqual(secondaryRecipe.Id, recipesToExecute.Single().Id);
        }

    }
}
