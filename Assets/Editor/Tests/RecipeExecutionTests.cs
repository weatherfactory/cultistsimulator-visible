using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NSubstitute;
using NSubstitute.Exceptions;

namespace CS.Tests
{

    
    [TestFixture]
   public  class RecipeExecutionTests
    {
        private INotifier notifier;
        private IElementsContainer elementsContainer;
        private Recipe primaryRecipe;
        private Recipe secondaryRecipe;
        private Recipe tertiaryRecipe;
        private RecipeAlternative recipeAlternative;
        private RecipeCompendium recipeCompendium;
        private IDice mockDice;
        


        [SetUp]
        public void Setup()
        {
            notifier = Substitute.For<INotifier>();
            elementsContainer = Substitute.For<IElementsContainer>();
            primaryRecipe = new Recipe {Id="primaryrecipe",
                Description = "A primary sort of description."};
            secondaryRecipe= new Recipe
            {
                Id = "secondaryrecipe",
                Description = "A more secondary description."
            };

            tertiaryRecipe = new Recipe
            {
                Id = "tertiaryrecipe",
                Description = "And a tertiary description."
            }; //this isn't normally added as an alternative


            primaryRecipe.Effects.Add("primaryrecipeeffect", 1);

            recipeAlternative = new RecipeAlternative(secondaryRecipe.Id, 50, false);
            primaryRecipe.AlternativeRecipes.Add(recipeAlternative);


            List<Recipe> allRecipes=new List<Recipe>() {primaryRecipe,secondaryRecipe,tertiaryRecipe};
            mockDice = Substitute.For<IDice>();

            recipeCompendium=new RecipeCompendium(allRecipes,mockDice);

        }

        [Test]
        public void RecipeDoubleDispatches()
        {
            primaryRecipe.Do(notifier, elementsContainer);
            notifier.Received(1).Log(primaryRecipe.Description, Style.Subtle);
            elementsContainer.Received(1).ModifyElementQuantity(primaryRecipe.Effects.Single().Key, primaryRecipe.Effects.Single().Value);
        }


        [Test]
        public void AlternateRecipeExecutes_IfNoRequirements_AndDiceRollSatisfied()
        {
            mockDice.Rolld100().Returns(recipeAlternative.Chance);
            List<Recipe> recipesToExecute=recipeCompendium.GetActualRecipesToExecute(primaryRecipe, elementsContainer);
            Assert.AreEqual(secondaryRecipe.Id,recipesToExecute.Single().Id);
        }


        [Test]
        public void AlternativeRecipeDoesntExecute_IfNoRequirements_AndDiceRollUnsatisfied()
        {
            mockDice.Rolld100().Returns(recipeAlternative.Chance+1);
            List<Recipe> recipesToExecute = recipeCompendium.GetActualRecipesToExecute(primaryRecipe, elementsContainer);
            Assert.AreEqual(primaryRecipe.Id, recipesToExecute.Single().Id);

        }


        [Test]
        public void AlternateRecipeExecutes_IfRequirementsSatisfied()
        {
            const string SECONDIUM = "secondium";
            const int SECONDIUM_REQUIRED = 10;
            mockDice.Rolld100().Returns(recipeAlternative.Chance);
            secondaryRecipe.Requirements.Add(SECONDIUM, SECONDIUM_REQUIRED);
            elementsContainer.GetCurrentElementQuantity(SECONDIUM).Returns(SECONDIUM_REQUIRED);

            List<Recipe> recipesToExecute = recipeCompendium.GetActualRecipesToExecute(primaryRecipe, elementsContainer);
            Assert.AreEqual(secondaryRecipe.Id, recipesToExecute.Single().Id);

        }

        [Test]
        public void AlternateRecipeDoesntExecute_IfRequirementsUnsatisfied()
        {
            const string SECONDIUM = "secondium";
            const int SECONDIUM_REQUIRED = 10;
            mockDice.Rolld100().Returns(recipeAlternative.Chance);
            secondaryRecipe.Requirements.Add(SECONDIUM, SECONDIUM_REQUIRED);
            elementsContainer.GetCurrentElementQuantity(SECONDIUM).Returns(SECONDIUM_REQUIRED-1);

            List<Recipe> recipesToExecute = recipeCompendium.GetActualRecipesToExecute(primaryRecipe, elementsContainer);
            Assert.AreEqual(primaryRecipe.Id, recipesToExecute.Single().Id);

        }

        [Test]
        public void AlternateRecipeMarkedAsAdditionalExecutesAfterFirst()
        {
            mockDice.Rolld100().Returns(1);
            primaryRecipe.AlternativeRecipes[0]=new RecipeAlternative(secondaryRecipe.Id,100,true);
            List<Recipe> recipesToExecute = recipeCompendium.GetActualRecipesToExecute(primaryRecipe, elementsContainer);
            
            Assert.AreEqual(primaryRecipe.Id, recipesToExecute[0].Id);
            Assert.AreEqual(secondaryRecipe.Id, recipesToExecute[1].Id);
        }

        [Test]
        public void TwoAlternateRecipesMarkedAsAdditionalExecuteAfterFirst()
        {
            primaryRecipe.AlternativeRecipes[0] = new RecipeAlternative(secondaryRecipe.Id, 100, true);
            primaryRecipe.AlternativeRecipes.Add(new RecipeAlternative(tertiaryRecipe.Id,100,true));
            mockDice.Rolld100().Returns(1);
            List<Recipe> recipesToExecute = recipeCompendium.GetActualRecipesToExecute(primaryRecipe, elementsContainer);
            Assert.AreEqual(primaryRecipe.Id, recipesToExecute[0].Id);
            Assert.AreEqual(secondaryRecipe.Id, recipesToExecute[1].Id);
            Assert.AreEqual(tertiaryRecipe.Id, recipesToExecute[2].Id);

        }

        [Test]
        public void AlternativeToAnAlternative_ExecutesInPlaceOfTheOriginal()
        { }

        
    }
}
