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
        private RecipeConductor rc;

     [SetUp]
        public void Setup()
        {

            compendium = Substitute.For<ICompendium>();
             rc = new RecipeConductor(compendium);
        }
        [Test]
        public void RecipeConductor_SuppliesLoopRecipeToCompletedSituation()
        {
            Recipe r1 = TestObjectGenerator.GenerateRecipe(1);
            Recipe r2 = TestObjectGenerator.GenerateRecipe(2);
            r1.Loop = r2.Id;

            compendium.GetRecipeById(r2.Id).Returns(r2);

            Assert.AreEqual(r2,rc.GetNextRecipe(r1));
        }

    }
}
