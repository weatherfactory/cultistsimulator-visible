using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.Logic;
using Noon;
using NSubstitute;
using NUnit.Framework;

namespace Assets.Editor.Tests
{

  [TestFixture]
    public class SituationEffectExecutorTests
    {
        private ISituationEffectCommand mockCommand;
        private IElementStacksManager mockStacksManager;
        private Recipe recipe;
        [SetUp]
        public void Setup()
        {
            NoonUtility.UnitTestingMode = true;

             mockCommand = Substitute.For<ISituationEffectCommand>();
            mockStacksManager=Substitute.For<IElementStacksManager>();
            recipe = TestObjectGenerator.GenerateRecipe(1);
            mockCommand.Recipe.ReturnsForAnyArgs(recipe);

            var mockNotifier = Substitute.For<INotifier>();
            var registry=new Registry();
            registry.Register<INotifier>(mockNotifier);

        }

        [Test]
        public void RecipeEffect_IsApplied()
        {
            mockCommand.GetElementChanges().ReturnsForAnyArgs(new Dictionary<string, int>
            { {"e", 1}});
            mockStacksManager.GetTotalAspects().Returns(new AspectsDictionary());

            var ex =new SituationEffectExecutor();
            var storage = Substitute.For<IGameEntityStorage>();

            ex.RunEffects(mockCommand, mockStacksManager,storage);
            mockStacksManager.Received().ModifyElementQuantity("e",1, Source.Fresh(),null);
        }
        [Test]
        public void RecipeDeckEffect_IsApplied()
        {
            string deckId = "did";
            //prep deckSpec to be drawn from
            IDeckInstance deckInstance = Substitute.For<IDeckInstance>();
            //ensure it returns 'e' element, whose change we'll see applied
            deckInstance.Draw().Returns("e");

            //return empty element changes for the recipe - so nothing will change on account of just the recipe
            mockCommand.GetElementChanges().ReturnsForAnyArgs(new Dictionary<string, int>());
            
            //but we return the deckid from the deckeffect - so we will try to retrieve that deckinstance
            mockCommand.GetDeckEffect().Returns(deckId);
            mockStacksManager.GetTotalAspects().Returns(new AspectsDictionary()); //need something in the aspects to satisfy the requirement to combine 'em

            //and we set up to return the deckSpec for that ID from the compendium
            var storage = Substitute.For<IGameEntityStorage>();
            
            storage.GetDeckInstanceById("did").Returns(deckInstance);

            var ex = new SituationEffectExecutor();

            ex.RunEffects(mockCommand, mockStacksManager,storage);

            mockStacksManager.Received().ModifyElementQuantity("e", 1, Source.Fresh(), null);

            

        }

        [Test]
        public void XTrigger_IsTriggeredByElementAspect()
        {
            mockCommand.GetElementChanges().ReturnsForAnyArgs(new Dictionary<string, int>());
            var estack1 = TestObjectGenerator.CreateElementCard("1",1);
            estack1.Element.XTriggers.Add("triggeraspect","alteredelement");
            mockStacksManager.GetStacks().Returns(new List<IElementStack> { estack1 });
            mockStacksManager.GetTotalAspects().Returns(new AspectsDictionary {{"triggeraspect", 1}});

            var ex = new SituationEffectExecutor();

            var storage = Substitute.For<IGameEntityStorage>();

            ex.RunEffects(mockCommand, mockStacksManager,storage);
            mockStacksManager.Received().ModifyElementQuantity("1", -1, Source.Existing(),null);
            mockStacksManager.Received().ModifyElementQuantity("alteredelement", 1, Source.Existing(), null);
        }

        [Test]
        public void XTrigger_IsTriggeredByRecipeAspect()
        {
            
            recipe.Aspects.Add("triggeraspect", 1);
            var estack1 = TestObjectGenerator.CreateElementCard("1", 1);
            estack1.Element.XTriggers.Add("triggeraspect", "alteredelement");

            mockCommand.GetElementChanges().ReturnsForAnyArgs(new Dictionary<string, int>());
            mockStacksManager.GetStacks().Returns(new List<IElementStack> { estack1 });
            mockStacksManager.GetTotalAspects().Returns(new AspectsDictionary ());
        

            var ex = new SituationEffectExecutor();

            var storage = Substitute.For<IGameEntityStorage>();

            ex.RunEffects(mockCommand, mockStacksManager,storage);
            mockStacksManager.Received().ModifyElementQuantity("1", -1, Source.Existing(), null);
            mockStacksManager.Received().ModifyElementQuantity("alteredelement", 1, Source.Existing(), null);
        }
    }
}
