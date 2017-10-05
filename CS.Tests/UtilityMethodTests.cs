using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    public class UtilityMethodTests
    {
        [Test]
        public void HashtableToStringIntDictionary()
        {
            var ht = new Hashtable {{"stringvalue", "3"}};

            var d = Noon.NoonUtility.HashtableToStringIntDictionary(ht);
            Assert.AreEqual("stringvalue",d.Keys.Single());
            Assert.AreEqual(3, d.Values.Single());
        }
    }
}
