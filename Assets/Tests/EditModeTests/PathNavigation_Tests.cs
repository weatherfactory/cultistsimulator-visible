﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
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
    public class PathNavigation_Tests
    {

        private HornedAxe _hornedAxe;
        private PrefabFactory _prefabFactory;
        private Sphere _sphereInRoot;
        private Sphere _OTHERSphereInRoot;
        private Token _tokenInSphere;
        private Sphere _sphereInTokenPayload;

        private const string SPHEREINROOT_ID = "sphereinroot";
        private const string OTHERSPHEREINROOT_ID = "othersphereinroot";
        private const string SPHEREINTOKENPAYLOAD_ID = "sphereintokenpayload";


        [SetUp]
        public void Setup()
        {
            FucineRoot.Reset();
            Watchman.ForgetEverything();
            
            _hornedAxe = Watchman.Get<HornedAxe>();
            _prefabFactory = Watchman.Get<PrefabFactory>();


            SphereSpec rootSphereSpec = new SphereSpec(typeof(MinimalSphere), SPHEREINROOT_ID );
             _sphereInRoot = _prefabFactory.InstantiateSphere(rootSphereSpec);

             SphereSpec otherRootSphereSpec = new SphereSpec(typeof(MinimalSphere), OTHERSPHEREINROOT_ID);
             _OTHERSphereInRoot = _prefabFactory.InstantiateSphere(otherRootSphereSpec);


            var tokenCreationCommand = new TokenCreationCommand();
             var minimalPayloadCreationCommand = new MinimalPayloadCreationCommand();
             tokenCreationCommand.Payload = minimalPayloadCreationCommand;

             _tokenInSphere = tokenCreationCommand.Execute(Context.Unknown(), _sphereInRoot);

             var sphereInTokenPayloadSpec = new SphereSpec(typeof(MinimalSphere), SPHEREINTOKENPAYLOAD_ID);

             _sphereInTokenPayload= _tokenInSphere.Payload.Dominions.First().TryCreateSphere(sphereInTokenPayloadSpec);
             _hornedAxe.RegisterSphere(_sphereInTokenPayload);

            
        }

        [Test]
        public void RetrieveRootSphere_ByPath()
        {
            var sphereinrootpath = new FucinePath("~/" + SPHEREINROOT_ID);
            var retrievedSphere = _hornedAxe.GetSphereByPath(sphereinrootpath);
            Assert.AreEqual(_sphereInRoot,retrievedSphere);

        }

        [Test]
        public void RetrieveTokenPayload_ByPath()
        {
            var tokenInSpherePath = _sphereInRoot.GetAbsolutePath().AppendToken(_tokenInSphere.PayloadId);
            var retrievedToken = _hornedAxe.GetTokenByPath(tokenInSpherePath);
            Assert.AreEqual(_tokenInSphere.PayloadId, retrievedToken.PayloadId);
        }

        [Test]
        public void RetrieveSphereInTokenPayload_ByPath()
        {
            var sphereInTokenPayloadPath = _sphereInRoot.GetAbsolutePath().AppendToken(_tokenInSphere.PayloadId)
                .AppendSphere(SPHEREINTOKENPAYLOAD_ID);
            var retrievedSphere = _hornedAxe.GetSphereByPath(sphereInTokenPayloadPath);
            Assert.AreEqual(_sphereInTokenPayload,retrievedSphere);
        }


        [Test]
        public void RetrieveTokenPayload_ByPath_AfterMovingToken()
        {
         _OTHERSphereInRoot.AcceptToken(_tokenInSphere,Context.Unknown());

         var tokenInSpherePath = _OTHERSphereInRoot.GetAbsolutePath().AppendToken(_tokenInSphere.PayloadId);
         var retrievedToken = _hornedAxe.GetTokenByPath(tokenInSpherePath);
         Assert.AreEqual(_tokenInSphere.PayloadId, retrievedToken.PayloadId);

        }

        [Test]
        public void RetrieveSphereInTokenPayload_ByPath_AfterMovingToken()
        {
            _OTHERSphereInRoot.AcceptToken(_tokenInSphere, Context.Unknown());

            var sphereInTokenPayloadPath = _OTHERSphereInRoot.GetAbsolutePath().AppendToken(_tokenInSphere.PayloadId)
                .AppendSphere(SPHEREINTOKENPAYLOAD_ID);
            var retrievedSphere = _hornedAxe.GetSphereByPath(sphereInTokenPayloadPath);
            Assert.AreEqual(_sphereInTokenPayload, retrievedSphere);
        }

        [Test]
        public void Payload_ReturnsCorrectAbsolutePath()
        {
            Assert.AreEqual("~/sphereinroot!defaultminimalpayloadid", _tokenInSphere.Payload.GetAbsolutePath().ToString());
        }

        [Test]
        public void Sphere_ReturnsCorrectAbsolutePath()
        {
            Assert.AreEqual("~/sphereinroot!defaultminimalpayloadid/sphereintokenpayload", _sphereInTokenPayload.GetAbsolutePath().ToString());
        }

        [Test]
        public void CanRetrieveSphereWithRelativePath()
        {
            FucinePath relativePath = new FucinePath("/sphereintokenpayload");
            var sphereRetrieved = Watchman.Get<HornedAxe>().GetSphereByPath(relativePath, _tokenInSphere.Payload);
            Assert.AreEqual(_sphereInTokenPayload,sphereRetrieved);
        }


        [Test]
        public void EmptyPathRetrievesCurrentSphere()
        {
            FucinePath relativePath = new FucinePath(String.Empty);
            var sphereRetrieved = Watchman.Get<HornedAxe>().GetSphereByPath(relativePath, _sphereInRoot);
            Assert.AreEqual(_sphereInRoot, sphereRetrieved);


        }

        [Test]
        public void PurgeSituations_WithPath()
        {
            throw new NotImplementedException();

        }



    }
}