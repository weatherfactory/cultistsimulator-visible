using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
   public class CommandAndSpecificationTests
    {
        [Test]
        public void Test_ElementQuantitySpecification_CalculatesDepthFromLocationPath()
        {
            var eqs0=new ElementStackSpecification("element",1,"primary",new Dictionary<string, int>(),  0);
            Assert.AreEqual(0,eqs0.Depth);

            var eqs1 = new ElementStackSpecification("element", 1, "primary_foo", new Dictionary<string, int>(), 0);
            Assert.AreEqual(1, eqs1.Depth);

            var eqs2 = new ElementStackSpecification("element", 1, "primary_foo_bar", new Dictionary<string, int>(), 0);
            Assert.AreEqual(2, eqs2.Depth);
        }
    }
}
