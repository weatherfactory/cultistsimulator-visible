using System.Collections.Generic;
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
            registry.Register<ICompendium>(Substitute.For<ICompendium>());
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
            mockStacksManager.Received().ModifyElementQuantity("e",1, Source.Fresh(), Arg.Is<Context>(c => c.actionSource == Context.ActionSource.SituationEffect));
        }
        [Test]
        public void RecipeDeckEffect_IsApplied()
        {
            string firstDeckElement1 = "deck1Element1";
            string secondDeckElement1 = "deck2Element1";
            string firstDeckId = "aDeckId";
            string secondDeckId = "anotherDeckId";
            Dictionary<string, int> deckIds = new Dictionary<string, int>
            {
                {firstDeckId, 1},
                {secondDeckId ,1}
            };
            //prep deckSpec to be drawn from
            IDeckInstance firstDeckInstance = Substitute.For<IDeckInstance>();
            IDeckInstance secondDeckInstance = Substitute.For<IDeckInstance>();
            //ensure it returns 'e' element, whose change we'll see applied
            firstDeckInstance.Draw().Returns(firstDeckElement1);
            secondDeckInstance.Draw().Returns(secondDeckElement1);

            //return empty element changes for the recipe - so nothing will change on account of just the recipe
            mockCommand.GetElementChanges().ReturnsForAnyArgs(new Dictionary<string, int>());

            //but we return the deckid from the deckeffect - so we will try to retrieve that deckinstance
            mockCommand.GetDeckEffects().Returns(deckIds);
            mockStacksManager.GetTotalAspects().Returns(new AspectsDictionary()); //need something in the aspects to satisfy the requirement to combine 'em

            //and we set up to return the deckSpec for that ID from the compendium
            var storage = Substitute.For<IGameEntityStorage>();

            storage.GetDeckInstanceById(firstDeckId).Returns(firstDeckInstance);
            storage.GetDeckInstanceById(secondDeckId).Returns(secondDeckInstance);


            var ex = new SituationEffectExecutor();

            ex.RunEffects(mockCommand, mockStacksManager,storage);

            mockStacksManager.Received().ModifyElementQuantity(firstDeckElement1, 1, Source.Fresh(), Arg.Is<Context>(c=>c.actionSource==Context.ActionSource.SituationEffect));
            mockStacksManager.Received().ModifyElementQuantity(secondDeckElement1, 1, Source.Fresh(), Arg.Is<Context>(c => c.actionSource == Context.ActionSource.SituationEffect));



        }

        [Test]
        public void XTrigger_IsTriggeredByElementAspect()
       {
           var r=new Registry();
           r.Register(new StackManagersCatalogue());
            mockCommand.GetElementChanges().ReturnsForAnyArgs(new Dictionary<string, int>());
            var estack1 = TestObjectGenerator.CreateElementCard("1",1);
            estack1.Element.XTriggers.Add("triggeraspect","alteredelement");
            mockStacksManager.GetStacks().Returns(new List<IElementStack> { estack1 });
            mockStacksManager.GetTotalAspects().Returns(new AspectsDictionary {{"triggeraspect", 1}});

            var ex = new SituationEffectExecutor();

            var storage = Substitute.For<IGameEntityStorage>();

            ex.RunEffects(mockCommand, mockStacksManager,storage);
            mockStacksManager.Received().ModifyElementQuantity("1", -1, Source.Existing(), Arg.Is<Context>(c => c.actionSource == Context.ActionSource.SituationEffect));
            mockStacksManager.Received().ModifyElementQuantity("alteredelement", 1, Source.Existing(), Arg.Is<Context>(c => c.actionSource == Context.ActionSource.SituationEffect));
        }

        [Test]
        public void XTrigger_IsTriggeredByRecipeAspect()
        {
            var r = new Registry();
            r.Register(new StackManagersCatalogue());
            mockCommand.GetElementChanges().ReturnsForAnyArgs(new Dictionary<string, int>());

            recipe.Aspects.Add("triggeraspect", 1);
            var estack1 = TestObjectGenerator.CreateElementCard("1", 1);
            estack1.Element.XTriggers.Add("triggeraspect", "alteredelement");

            mockCommand.GetElementChanges().ReturnsForAnyArgs(new Dictionary<string, int>());
            mockStacksManager.GetStacks().Returns(new List<IElementStack> { estack1 });
            mockStacksManager.GetTotalAspects().Returns(new AspectsDictionary ());


            var ex = new SituationEffectExecutor();

            var storage = Substitute.For<IGameEntityStorage>();

            ex.RunEffects(mockCommand, mockStacksManager,storage);
            mockStacksManager.Received().ModifyElementQuantity("1", -1, Source.Existing(), Arg.Is<Context>(c => c.actionSource == Context.ActionSource.SituationEffect));
            mockStacksManager.Received().ModifyElementQuantity("alteredelement", 1, Source.Existing(), Arg.Is<Context>(c => c.actionSource == Context.ActionSource.SituationEffect));
        }
    }
}
