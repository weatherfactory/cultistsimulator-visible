using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.Spheres.SecretHistories.Spheres;
using SecretHistories.UI;

namespace Assets.Tests.EditModeTests
{
    [TestFixture]
    public class GetAbsolutePath_Tests
    {
        private HornedAxe _hornedAxe;
        private PrefabFactory _prefabFactory;
        private Sphere _sphereInRoot;
        private Sphere _OTHERSphereInRoot;
        private Token _tokenInSphere;
        private Sphere _sphereInTokenPayload;

        [SetUp]
        public void Setup()
        {

            Watchman.ForgetEverything();
            _hornedAxe = Watchman.Get<HornedAxe>();
            _prefabFactory = Watchman.Get<PrefabFactory>();

            SphereSpec rootSphereSpec = new SphereSpec(typeof(MinimalSphere),  "rootSphere");
            _sphereInRoot = _prefabFactory.InstantiateSphere(rootSphereSpec);
            _hornedAxe.RegisterSphere(_sphereInRoot);

            SphereSpec otherRootSphereSpec = new SphereSpec(typeof(MinimalSphere), "sphereInRoot");
            _OTHERSphereInRoot = _prefabFactory.InstantiateSphere(otherRootSphereSpec);
            _hornedAxe.RegisterSphere(_OTHERSphereInRoot);


            var tokenCreationCommand = new TokenCreationCommand();
            var minimalPayloadCreationCommand = new MinimalPayloadCreationCommand();
            minimalPayloadCreationCommand.Id = "minimalPayload";
            tokenCreationCommand.Payload = minimalPayloadCreationCommand;

            _tokenInSphere = tokenCreationCommand.Execute(Context.Unknown(), _sphereInRoot);

            var sphereInTokenPayloadSpec = new SphereSpec(typeof(MinimalSphere), "sphereInTokenPayload");

            _sphereInTokenPayload = _tokenInSphere.Payload.Dominions.First().CreateSphere(sphereInTokenPayloadSpec);
            _hornedAxe.RegisterSphere(_sphereInTokenPayload);
        }

        [Test]
        public void Payload_ReturnsCorrectAbsolutePath()
        {
            Assert.AreEqual("./rootSphere!minimalPayload",_tokenInSphere.Payload.GetAbsolutePath().ToString());
        }

        [Test]
        public void Sphere_ReturnsCorrectAbsolutePath()
        {
            Assert.AreEqual("./rootSphere!minimalPayload/sphereInTokenPayload", _sphereInTokenPayload.GetAbsolutePath().ToString());
        }

    }
}
