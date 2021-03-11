using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.Spheres.SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEditorInternal;
using UnityEngine;

namespace Assets.Tests.EditModeTests
{
    [TestFixture]
 public class SituationFlowTests
    {
        private HornedAxe _hornedAxe;
        private PrefabFactory _prefabFactory;
        private Sphere _sphereInRoot;
        private Sphere _minimalTabletopSurrogate;
        private Sphere _minimalWindowsSurrogate;

        [SetUp]
        public void Setup()
        {

            Watchman.ForgetEverything();
            FucineRoot.Reset();

            var compendium = new Compendium();
            var cl = new CompendiumLoader("testcontent");
            var importLog = cl.PopulateCompendium(compendium, "en");
            if (importLog.ImportFailed())
            {
                throw new ApplicationException(string.Concat(importLog.GetMessages()));
            }

            var watchman = new Watchman();
            watchman.Register(compendium);
            var gobjStable = new GameObject();
            var stableComponent = gobjStable.AddComponent<Stable>();
            watchman.Register(stableComponent);

            _hornedAxe = Watchman.Get<HornedAxe>();

            var defaultSphereSpec = new SphereSpec(typeof(MinimalSphere), "tabletop");
            defaultSphereSpec.WindowsSpherePath = new FucinePath("~/windows");
            _minimalTabletopSurrogate = Watchman.Get<PrefabFactory>().InstantiateSphere(defaultSphereSpec);
            var windowsSphereSpec = new SphereSpec(typeof(MinimalSphere), "windows");
            _minimalWindowsSurrogate = Watchman.Get<PrefabFactory>().InstantiateSphere(windowsSphereSpec);

        }

        [Test]
        public void ActivateRecipeManually()
        {
            SituationCreationCommand situationCreationCommand = new SituationCreationCommand("t");
            situationCreationCommand.StateForRehydration = StateEnum.Unstarted;
            var situation = situationCreationCommand.Execute(Context.Unknown()) as Situation;

            var activationCommand = TryActivateRecipeCommand.ManualRecipeActivation("req0effect1");
            situation.CommandQueue.AddCommand(activationCommand);
            Assert.AreEqual(StateEnum.Unstarted, situation.StateForRehydration);
            situation.ExecuteHeartbeat(1f);
            Assert.AreEqual(StateEnum.Ongoing, situation.StateForRehydration);

        }

        }
}
