using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.Spheres.SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Tests.EditModeTests
{

    [TestFixture]
    public class HornedAxe_Tests
    {

        private HornedAxe _hornedAxe;
        private PrefabFactory _prefabFactory;
        private Sphere _sphereInRoot;

        private const string SPHEREINROOT_ID = "sphereinroot";

        [SetUp]
        public void Setup()
        {
            Watchman.ForgetEverything();
            _hornedAxe = Watchman.Get<HornedAxe>();
            _prefabFactory = Watchman.Get<PrefabFactory>();
            SphereSpec rootSphereSpec = new SphereSpec(typeof(MinimalSphere), SPHEREINROOT_ID );
             _sphereInRoot = _prefabFactory.InstantiateSphere(rootSphereSpec);
             _hornedAxe.RegisterSphere(_sphereInRoot);
        }

        [Test]
        public void RetrieveRootSphere_ByPath()
        {
            var sphereinrootpath = new FucinePath("./" + SPHEREINROOT_ID);
            var retrievedSphere = _hornedAxe.GetSphereByPath(sphereinrootpath);
            Assert.AreEqual(_sphereInRoot,retrievedSphere);

        }

        [Test]
        public void RetrieveTokenPayload_ByPath()
        {
            throw new NotImplementedException();


        }

        [Test]
        public void RetrieveSphereInTokenPayload_ByPath()
        {
            throw new NotImplementedException();
        }


        [Test]
        public void RetrieveTokenPayload_ByPath_AfterMovingToken()
        {
            throw new NotImplementedException();

        }

        [Test]
        public void RetrieveSphereInTokenPayload_ByPath_AfterMovingToken()
        {
            throw new NotImplementedException();

        }
    }
}
