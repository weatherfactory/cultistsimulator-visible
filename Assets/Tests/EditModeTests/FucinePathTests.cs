using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SecretHistories.Fucine;

namespace Assets.Tests.EditModeTests
{
    [TestFixture]
   public class FucinePathTests
    {

        [Test]
        public void SituationPath_CanBeSpecifiedWithBang()
        {
            var situationPath = new SituationPath("!foo");
            Assert.AreEqual(situationPath.ToString(),"!foo");
        }

        [Test]
        public void SituationPath_SpecifiedWithoutABang_PrependsBang()
        {
            var situationPath = new SituationPath("foo");
            Assert.AreEqual(situationPath.ToString(), "!foo");

        }


        [Test]
        public void SituationPath_CanBeIdentifiedInSpherePath()
        {
            var spherePath = new SpherePath("!spath/sphereid");
            Assert.AreEqual(spherePath.GetBaseSituationPath(), new SituationPath("!spath"));
        }

        [Test]
        public void RootSituationPath_CanBeIdentifiedInSpherePath()
        {
            var spherePath = new SpherePath("!./tabletop/spath/sphereid");
            Assert.AreEqual(spherePath.GetBaseSituationPath(), new SituationPath("!."));
        }

    }
}
