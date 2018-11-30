using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Noon;
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
        private LinkedRecipeDetails _linkedRecipeDetails;
        private RecipeConductor rc;

        [SetUp]
        public void Setup()
        {
            NoonUtility.UnitTestingMode = true;

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

            _linkedRecipeDetails = new LinkedRecipeDetails(secondaryRecipe.Id, 50, false,null);
            primaryRecipe.AlternativeRecipes.Add(_linkedRecipeDetails);


            List<Recipe> allRecipes = new List<Recipe>() { primaryRecipe, secondaryRecipe, tertiaryRecipe, quaternaryRecipe };
            compendium = new Compendium();
            compendium.UpdateRecipes(allRecipes);
            mockDice = Substitute.For<IDice>();
        }
        [Test]
        public void RecipeConductor_SuppliesLoopRecipeToCompletedSituation()
        {
            LinkedRecipeDetails lrd=new LinkedRecipeDetails(primaryRecipe.Id,100,false, null);
            primaryRecipe.LinkedRecipes.Add(lrd);
           
           rc = new RecipeConductor(compendium, null,mockDice,new Character(null));
           var loopedRecipe = rc.GetLinkedRecipe(primaryRecipe);

           Assert.AreEqual(primaryRecipe,loopedRecipe);
        }

        [Test]
        public void RecipeConductor_SuppliesSecondLoopRecipeIfDiceRollForFirstFails()
        {
            LinkedRecipeDetails lrd1 = new LinkedRecipeDetails(primaryRecipe.Id, 99, false, null);
            LinkedRecipeDetails lrd2 = new LinkedRecipeDetails(secondaryRecipe.Id, 100, false, null);
            primaryRecipe.LinkedRecipes.Add(lrd1);
            primaryRecipe.LinkedRecipes.Add(lrd2);


            mockDice.Rolld100(Arg.Any<Recipe>()).Returns(100);
            rc =new RecipeConductor(compendium,null, mockDice, new Character(null));
            var loopedRecipe = rc.GetLinkedRecipe(primaryRecipe);

            Assert.AreEqual(secondaryRecipe.Id, loopedRecipe.Id);
        }


    }
}
