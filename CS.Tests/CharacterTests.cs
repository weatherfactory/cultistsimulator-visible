using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
    public class CharacterTests
    {
        [SetUp]
        public void Setup()
        { }

        [Test]
        public void Character_BelowMaxExecutionsForRecipe_HasNotExhaustedRecipe()
        {
            var r = TestObjectGenerator.GenerateRecipe(1);
            r.MaxExecutions = 2;
            var c=new Character(null);
            c.AddExecutionsToHistory(r.Id,1);
            Assert.IsFalse(c.HasExhaustedRecipe(r));

        }

        [Test]
        public void Character_AtMaxExecutionsForRecipe_HasExhaustedRecipe()
        {
            var r = TestObjectGenerator.GenerateRecipe(1);
            r.MaxExecutions = 1;
            var c = new Character(null);
            c.AddExecutionsToHistory(r.Id,1);
            Assert.IsTrue(c.HasExhaustedRecipe(r));

        }

        [Test]
        public void Character_WithMoreThanZeroExecutionsForRecipe_DoesNotExhaustInfiniteExecutionsRecipe()
        {
            var r = TestObjectGenerator.GenerateRecipe(1);
            r.MaxExecutions = 0;
            var c = new Character(null);
            c.AddExecutionsToHistory(r.Id,1);
            Assert.IsFalse(c.HasExhaustedRecipe(r));

        }
    }
}
