using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Fucine;
using NUnit.Framework;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;

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

        //I don't think we need the subclasses after all
        [Test]
        public void FucinePath_WithRootAsOnlyMember_IsAbsolute()
        {
            var absolutePath = new FucinePath(".");
            Assert.IsTrue(absolutePath.IsAbsolute());
        }

        //I don't think we need the subclasses after all
        [Test]
        public void FucinePath_WithRootInFirstPosition_IsAbsolute()
        {
            var absolutePath=new FucinePath("./tabletop");
            Assert.IsTrue(absolutePath.IsAbsolute());
        }

        [Test]
        public void FucinePath_WithoutRootInFirstPosition_IsNotAbsolute()
        {
            var relativePath=new FucinePath("/somesphereid");
            Assert.IsFalse(relativePath.IsAbsolute());

        }

        [Test]
        public void FucinePath_WithTokenXAtEndOfList_ReturnsXAsToken()
        {
            throw new NotImplementedException();

        }

        [Test]
        public void FucinePath_WithSphereAtEndOfList_ReturnsPreviousToken_AsToken()
        {
            throw new NotImplementedException();

        }

        [Test]
        public void FucinePath_WithSphereYAtEndOfList_ReturnsYAsSphere()
        {
            throw new NotImplementedException();

        }

        [Test]
        public void FucinePath_WithTokenAtEndOfList_ReturnsPreviousYAsSphere()
        {
            throw new NotImplementedException();

        }

        [Test]
        public void RootPathFollowedBySituationIsInvalid()
        {
            throw new NotImplementedException();
        }


    }
}
