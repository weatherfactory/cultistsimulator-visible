using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Assets.Tests.UnitTests
{
    [TestFixture]
    public class PlacementBehaviour
    {
        [Test]
        public void SphereCanOverrideTokenReturnToHomeIfOccupied()
        {
           // var originSphere = Registry.Get<SphereCatalogue>().GetSphereByPath(LocationBeforeDrag.AtSpherePath);

          //  originSphere.Choreographer.PlaceTokenAsCloseAsPossibleToSpecifiedPosition(this, context, LocationBeforeDrag.Position);
          Assert.AreEqual(true,false);
          
        }
    }
}
