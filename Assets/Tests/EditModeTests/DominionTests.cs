using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres.SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace Assets.Tests.EditModeTests
{
    [TestFixture]
   public class DominionTests
    {
        [Test]
        public void SphereWIthDuplicateId_CannotBeCreatedOnSituationDominion()
        {
            var situationDominion = new GameObject().AddComponent<SituationDominion>();
            var sphereSpec = new SphereSpec(typeof(MinimalSphere), "idwillbeduplicate");
            situationDominion.TryCreateSphere(sphereSpec);
            situationDominion.TryCreateSphere(sphereSpec);
            Assert.AreEqual(1, situationDominion.Spheres.Count);
        }

    }
}
