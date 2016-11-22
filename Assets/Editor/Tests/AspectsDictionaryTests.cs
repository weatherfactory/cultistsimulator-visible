using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
    public class AspectsDictionaryTests
    {
        [Test]
        public void AspectsDictionary_AddsNewAspects()
        {
            AspectsDictionary first=new AspectsDictionary();
            first.Add("a",1);
           AspectsDictionary second=new AspectsDictionary();
            second.Add("b",1);
            second.Add("c", 1);

            first.CombineAspects(second);
            Assert.AreEqual(3,first.Keys.Count);

            Assert.IsTrue(first["b"]==1);
            Assert.IsTrue(first["c"] == 1);
        }

        [Test]
        public void AspectsDictionary_CombinesExistingAspects()
        {
            AspectsDictionary first = new AspectsDictionary();
            first.Add("a", 1);
            first.Add("b",2);
            AspectsDictionary second = new AspectsDictionary();
            second.Add("a", 2);
            second.Add("b", 3);

            first.CombineAspects(second);
            Assert.AreEqual(2, first.Keys.Count);

            Assert.IsTrue(first["a"] == 3);
            Assert.IsTrue(first["b"] == 5);
        }
    }
}
