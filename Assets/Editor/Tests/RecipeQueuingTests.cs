using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
    public class RecipeQueuingTests
    {
        private World w;
        private RecipeCompendium rc;
        private Recipe r1;
        private Character c;
        private Element ePossessed;
        private const int EPOSSESSED_QUANTITY_IN_WORKSPACE = 3;
        private Element eAffected;
        private IDice dice;
        private const string ASPECT_1 = "A1";

        [SetUp]
        public void S()
        {
            ePossessed=new Element("E1","E1Label","E1Description");
            eAffected = new Element("E2", "E2Label", "E2Description");

            r1 = new Recipe()
            {
                Id = "BasicRecipe",
                Warmup = 1,
                Loop="BasicRecipe",
                Effects = new Dictionary<string, int>() { { eAffected.Id, 1 } },
                PersistedIngredients = new Dictionary<string, int>() { { ASPECT_1,1} }

        };

            rc = new RecipeCompendium(new List<Recipe>(){r1},dice );
            w = new World(rc);
            c = Substitute.For<Character>();
            c.GetCurrentElementQuantityInWorkspace(ePossessed.Id).Returns(EPOSSESSED_QUANTITY_IN_WORKSPACE);
        }

        [Test]
        public void RecipeWillNotQueue_IfMatchingRecipeSituationExists()
        {

            w.AddSituation(r1, r1.Warmup, c);
            w.FastForward(2);
            RecipeSituation rs=w.AddSituation(r1,r1.Warmup,c);
            Assert.IsNull(rs);

        }
        [Test]
        public void RecipeWithPersistentIngredients_WillAddIngredientsToRecipeSituationElements()
        {
            RecipeSituation rs=w.AddSituation(r1, r1.Warmup, c);
            Assert.AreEqual(EPOSSESSED_QUANTITY_IN_WORKSPACE,
                rs.ElementsContainerAffected.GetCurrentElementQuantity(ePossessed.Id));

        }
    }
}
