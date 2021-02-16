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
        public void SituationPath_CanBeIdentifiedInSpherePath()
        {
            var spherePath = new SpherePath("situationid_sphereid");
            Assert.AreEqual(spherePath.GetBaseSituationPath(), new SituationPath("situationid"));
        }

    }
}
