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
        private Compendium rc;
        private Recipe r1;
        private Character c;
        private Element e1;
        private Element e2;
        private const int E1_QUANTITY_IN_WORKSPACE = 3;
        private const int E2_QUANTITY_IN_WORKSPACE = 3;
        private Element e3;
        private IDice dice;
        private const string PermittedAspect = "A1";

        [SetUp]
        public void S()
        {
            e1=new Element("E1","E1Label","E1Description");
            e1.Aspects.Add(PermittedAspect, 1);
            e2 = new Element("E2", "E2Label", "E2Description");
            e3 = new Element("E3", "E3Label", "E3Description");

            Dictionary<string, Element> elements = new Dictionary<string, Element>()
            {
                {e1.Id, e1},
                {e2.Id, e2},
                {e3.Id, e3}
            };

            r1 = new Recipe()
            {
                Id = "BasicRecipe",
                Warmup = 1,
                Loop="BasicRecipe",
                Effects = new Dictionary<string, int>() { { e3.Id, 1 } },
                PersistsIngredientsWith = new Dictionary<string, int>() { { PermittedAspect,1} }

        };

            rc = new Compendium(dice);
            rc.UpdateRecipes(new List<Recipe>() { r1 });
            rc.UpdateElements(elements);
            w = new World(rc);
            c = Substitute.For<Character>();

        }

        [Test]
        public void RecipeWillNotQueue_IfMatchingRecipeSituationExists()
        {
            c.GetOutputElements().Returns(new Dictionary<string, int>()
            {
                { e1.Id,E1_QUANTITY_IN_WORKSPACE},
                {e2.Id,E2_QUANTITY_IN_WORKSPACE }
            });
            w.AddSituation(r1, r1.Warmup, c);
            w.FastForward(2);
            IRecipeSituation rs=w.AddSituation(r1,r1.Warmup,c);
            Assert.IsNull(rs);

        }
        [Test]
        public void RecipeWithPersistentIngredients_WillAddIngredientsToRecipeSituationContents()
        {
            c.GetOutputElements().Returns(new Dictionary<string, int>()
            {
                { e1.Id,E1_QUANTITY_IN_WORKSPACE},
                {e2.Id,E2_QUANTITY_IN_WORKSPACE }
            });

            IRecipeSituation rs =w.AddSituation(r1, r1.Warmup, c);
            Assert.AreEqual(E1_QUANTITY_IN_WORKSPACE,
                rs.GetInternalElementQuantity(e1.Id));

        }

        [Test]
        public void RecipeWithPersistentIngredients_WillOnlyAddFilterMatchingIngredientsToRecipeSituationContents()
        {
            c.GetOutputElements().Returns(new Dictionary<string, int>()
            {
                { e1.Id,E1_QUANTITY_IN_WORKSPACE},
                {e2.Id,E2_QUANTITY_IN_WORKSPACE }
            });

            IRecipeSituation rs = w.AddSituation(r1, r1.Warmup, c);
            Assert.AreEqual(E1_QUANTITY_IN_WORKSPACE,
                rs.GetInternalElementQuantity(e1.Id));
            Assert.AreEqual(0,
                rs.GetInternalElementQuantity(e2.Id));
        }

        [Test]
        public void RecipeWithPersistentIngredients_WillOnlyMatchConsumableIngredientsToRecipeSituationContents()
        {
            //give the second element the permitted aspect, but also add a slot specification that means it won't be consumed
            e2.Aspects.Add(PermittedAspect, 1);
            e2.ChildSlotSpecifications.Add(new ChildSlotSpecification("1"));
            
            c.GetOutputElements().Returns(new Dictionary<string, int>()
            {
                { e1.Id,E1_QUANTITY_IN_WORKSPACE},
                {e2.Id,E2_QUANTITY_IN_WORKSPACE }
            });


            IRecipeSituation rs = w.AddSituation(r1, r1.Warmup, c);
            Assert.AreEqual(E1_QUANTITY_IN_WORKSPACE,
                rs.GetInternalElementQuantity(e1.Id));
            Assert.AreEqual(0,
                rs.GetInternalElementQuantity(e2.Id));
        }
    }
}
