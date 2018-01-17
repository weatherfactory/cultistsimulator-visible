using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
    public class TestTest
    {
        [SetUp]
        public void Setup()
        { }

        [Test]
        public void TestTestTest()
        {
            var r = 1;
            Assert.AreEqual(1,r);

        }


    }
}
