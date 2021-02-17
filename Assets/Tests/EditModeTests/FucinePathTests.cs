using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Fucine;
using NUnit.Framework;
using SecretHistories.Fucine;

namespace Assets.Tests.EditModeTests
{
    [TestFixture]
   public class FucinePathTests
    {

        [Test]
        public void TokenPathId_CanBeSpecifiedWithBang()
        {
            var tokenPathId = new TokenPathId("!foo");
            Assert.AreEqual("!foo", tokenPathId.ToString());
        }

        [Test]
        public void TokenPathId_SpecifiedWithoutABang_PrependsBang()
        {
            var tokenPathId = new TokenPathId("foo");
            Assert.AreEqual( "!foo", tokenPathId.ToString());

        }


        [Test]
        public void SpherePathId_CanBeSpecifiedWithForwardSlash()
        {
            var spherePathId=new SpherePathId("/foo");
            Assert.AreEqual("/foo",spherePathId.ToString());
        }

        [Test]
        public void SpherePathId_SpecifiedWithoutForwardSlash_PrependsSlash()
        {
            var spherePathId = new SpherePathId("foo");
            Assert.AreEqual("/foo", spherePathId.ToString());

        }




    }
}
