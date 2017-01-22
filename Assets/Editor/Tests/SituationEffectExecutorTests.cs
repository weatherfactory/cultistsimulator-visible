using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.Logic;
using NSubstitute;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
    public class SituationEffectExecutorTests
    {
        private ISituationEffectCommand mockCommand;
        private IElementStacksManager mockStacksManager;
        [SetUp]
        public void Setup()
        {
             mockCommand = Substitute.For<ISituationEffectCommand>();
            mockStacksManager=Substitute.For<IElementStacksManager>();
        }

        [Test]
        public void RecipeEffect_IsApplied()
        {
            mockCommand.GetElementChanges().ReturnsForAnyArgs(new Dictionary<string, int>
            { {"e", 1}});
            var ex=new SituationEffectExecutor();
            ex.RunEffects(mockCommand, mockStacksManager);
            mockStacksManager.Received().ModifyElementQuantity("e",1);
        }
        [Test]
        public void XTrigger_IsTriggeredByElementEffect()
        {
            mockCommand.GetElementChanges().ReturnsForAnyArgs(new Dictionary<string, int>());
            var estack1 = TestObjectGenerator.CreateElementCard("1",1);
            estack1.Element.XTriggers.Add("triggeraspect","alteredelement");
            mockStacksManager.GetStacks().Returns(new List<IElementStack> { estack1 });
            mockStacksManager.GetTotalAspects().Returns(new AspectsDictionary {{"triggeraspect", 1}});

            var ex = new SituationEffectExecutor();


            ex.RunEffects(mockCommand, mockStacksManager);
            mockStacksManager.Received().ModifyElementQuantity("1", -1);
            mockStacksManager.Received().ModifyElementQuantity("alteredelement", 1);
        }
    }
}
