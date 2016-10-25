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

            primaryRecipe.Effects.Add("primaryrecipeeffect", 1);

            recipeAlternative = new RecipeAlternative(secondaryRecipe.Id, 50, false);
            primaryRecipe.AlternativeRecipes.Add(recipeAlternative);


            List<Recipe> allRecipes=new List<Recipe>() {primaryRecipe,secondaryRecipe};
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



        // alternativerecipes: //these will be completed in place of this recipe if (1) their requirements are satisfied *by concrete possessed resources, not considering those resources' aspects;
        //and (2) if we roll <=chance on d100"
        //if additional=true, they'll execute as well as, not instead of, the original recipe
        //loop: recipeid //this, or another recipe, may begin when this completes. NB if an alternative recipe is triggered, the loop from that will apply instead.
    }
}
