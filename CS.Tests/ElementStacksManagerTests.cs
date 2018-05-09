using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Noon;
using NSubstitute;
using NUnit.Framework;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.Editor.Tests
{
    [TestFixture]
    public class ElementStacksManagerTests
    {
        private List<IElementStack> stacks;
        private ITokenContainer wrapper;
        private Registry registry;
        private ElementStacksManager elementStacksManager;
        [SetUp]
        public void Setup()
        {
            NoonUtility.UnitTestingMode = true;
            var stackManagersCatalogue = new StackManagersCatalogue();
            registry = new Registry();
            registry.Register<StackManagersCatalogue>(stackManagersCatalogue);

            stacks = TestObjectGenerator.CardsForElements(TestObjectGenerator.ElementDictionary(1, 3));
            wrapper = Substitute.For<ITokenContainer>();
         //   wrapper.GetStacks().Returns(stacks);

            elementStacksManager=new ElementStacksManager(wrapper,"testManager");

            elementStacksManager.AcceptStacks(stacks, null);

        }

        [Test]
        public void ReduceElement_AffectsAspectPossessor_WhenNoConcreteElementFound()
        {
          
            FakeElementStack stackToRemove = stacks[0] as FakeElementStack;
            stackToRemove.Element.Aspects.Add("pureAspect",1);
            elementStacksManager.ReduceElement("pureAspect", -1, null);
            Assert.IsTrue(stackToRemove.Defunct);
        }

        [Test]
        public void Manager_SumsUniqueElements()
        {

            var d = elementStacksManager.GetCurrentElementTotals();
            foreach(var c in stacks)
            {
                Assert.AreEqual(1, d[c.EntityId]);
                
            }
        }

        [Test]
        public void Manager_SumsExtraNonUniqueElements()
        {
         IElementStack newStack= TestObjectGenerator.CreateElementCard(stacks[0].EntityId,1);

            elementStacksManager.AcceptStack(newStack, null);

            var d = elementStacksManager.GetCurrentElementTotals();
             Assert.AreEqual(2,d[stacks[0].EntityId]);
            Assert.AreEqual(1, d[stacks[1].EntityId]);
            Assert.AreEqual(1, d[stacks[2].EntityId]);

        }


        [Test]
        public void Manager_SumsUniqueAspects()
        {
            var elements = TestObjectGenerator.ElementDictionary(1,2);
            TestObjectGenerator.AddAnAspectToEachElement(elements,"a");
            var aspectedCards = TestObjectGenerator.CardsForElements(elements);
       
            elementStacksManager.AcceptStacks(aspectedCards, null);
         
            var d = elementStacksManager.GetTotalAspects();
            Assert.AreEqual(2, d["1"]);
            Assert.AreEqual(1, d["a1"]);
            Assert.AreEqual(2, d["2"]);
            Assert.AreEqual(1, d["a2"]);
        }

        [Test]
        public void Manager_SumsDuplicateAspects()
        {
            var elements = TestObjectGenerator.ElementDictionary(1, 2);
            TestObjectGenerator.AddAnAspectToEachElement(elements,"a");
            elements["1"].Aspects.Add("a2",1);
            var aspectedCards = TestObjectGenerator.CardsForElements(elements);
            elementStacksManager.AcceptStacks(aspectedCards, null);
            var d = elementStacksManager.GetTotalAspects();
            Assert.AreEqual(1, d["a1"]);
            Assert.AreEqual(2, d["a2"]);
        }
    

    [Test]
    public void Manager_ReduceElement_CanOnlyTakeNegativeArgument()
    {
                  
            Assert.Throws<ArgumentException>(() => elementStacksManager.ReduceElement("1", 0, null));
            Assert.Throws<ArgumentException>(() => elementStacksManager.ReduceElement("1", 1, null));
    }

        [Test]
        public void Manager_ReduceElement_CallsRemoveOnSingleCard()
        {
            
            FakeElementStack stackToRemove = stacks[0] as FakeElementStack;
            elementStacksManager.ReduceElement(stackToRemove.EntityId,-1, null);
            Assert.IsTrue(stackToRemove.Defunct);
        }

        [Test]
        public void Manager_ReduceElementBy2_Removes2SingleCards()
        {
            
            FakeElementStack firstStackToRemove = stacks[0] as FakeElementStack;
            FakeElementStack secondStackToRemove = stacks[1] as FakeElementStack;
            secondStackToRemove.Element = firstStackToRemove.Element;
                
            
            Assert.AreEqual(0, elementStacksManager.ReduceElement(firstStackToRemove.EntityId, -2, null));
            Assert.IsTrue(firstStackToRemove.Defunct);
            Assert.IsTrue(secondStackToRemove.Defunct);
            //1 stack remaining out of 3
            Assert.AreEqual(1,stacks.Count(s => s.Defunct==false));
        }

        [Test]
        public void Manager_ReduceElementBy3_Removes2SingleCardsAndReturnsMinus1()
        {
         
            FakeElementStack firstStackToRemove = stacks[0] as FakeElementStack;
            FakeElementStack secondStackToRemove = stacks[1] as FakeElementStack;
            secondStackToRemove.Element = firstStackToRemove.Element;


            Assert.AreEqual(-1, elementStacksManager.ReduceElement(firstStackToRemove.EntityId, -3, null));
            Assert.IsTrue(firstStackToRemove.Defunct);
            Assert.IsTrue(secondStackToRemove.Defunct);

            //1 stack remaining out of 3
            Assert.AreEqual(1, stacks.Count(s => s.Defunct == false));          
        }

        [Test]
        public void Manager_IncreaseElement_CanOnlyTakePositiveArgument()
        {
          
            Assert.Throws<ArgumentException>(() => elementStacksManager.IncreaseElement("1", 0,Source.Existing(), null));
            Assert.Throws<ArgumentException>(() => elementStacksManager.IncreaseElement("1", -1,Source.Existing(), null));
        }

        [Test]
        public void Manager_IncreaseNewElementBy2_AddsNewStackOf2()
        {
         
            FakeElementStack newStack = TestObjectGenerator.CreateElementCard(stacks.Count + 1.ToString(), 2);
            wrapper.ProvisionElementStack(newStack.EntityId, newStack.Quantity,Source.Existing()).Returns(newStack);
            elementStacksManager.IncreaseElement(newStack.EntityId,newStack.Quantity,Source.Existing(), null);

            wrapper.Received().ProvisionElementStack(newStack.EntityId, newStack.Quantity,Source.Existing());
        }

        [Test]
        public void Manager_AcceptsStack()
        {

         
            FakeElementStack newStack =TestObjectGenerator.CreateElementCard(stacks.Count+1.ToString(),2);
            elementStacksManager.AcceptStack(newStack,null);
            wrapper.Received().DisplayHere(newStack,null);

        }


        
    }

    //I should mock this, actually
    public class FakeElementStack : IElementStack
    {
        public Element Element { get; set; }
        public string EntityId { get { return Element.Id; } }

        public string SaveLocationInfo { get; set; }
        public int Quantity { get; set; }
        public bool Defunct { get; private set; }
        public bool MarkedForConsumption { get; set; }

        public bool Decays { get; private set; }
        private IElementStacksManager CurrentStacksManager; 

        public FakeElementStack() {
            CurrentStacksManager=new ElementStacksManager(null,"fake"); //must be assigned to a stacksmanager at birth
        }

        public void SetStackManager(IElementStacksManager manager) {
            CurrentStacksManager = manager;
        }

        public IAspectsDictionary GetAspects(bool includingSelf = true)
        {
            if (includingSelf)
                return Element.AspectsIncludingSelf;
            else
                return Element.Aspects;
        }

        public Dictionary<string, int> GetCurrentMutations()
        {
            throw new NotImplementedException();
        }

        public void SetMutation(string aspectId, int value, bool additive = true)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetXTriggers()
        {
            return Element.XTriggers;
        }

        public void SetQuantity(int quantity)
        {
            Quantity = quantity;
        }

        public void Populate(string elementId, int quantity, Source source)
        {
            throw new NotImplementedException();
        }

        public void SignalRemovedFromContainer()
        {
        }

        public void Populate(string elementId, int quantity)
        {
            Element = TestObjectGenerator.CreateElement(int.Parse(elementId));
            Quantity = quantity;
        }

        public List<SlotSpecification> GetChildSlotSpecificationsForVerb(string forVerb)
        {
            throw new NotImplementedException();
        }

        public bool HasChildSlotsForVerb(string forVerb)
        {
            throw new NotImplementedException();
        }

        public IElementStack SplitAllButNCardsToNewStack(int n, Context context)
        {
            throw new NotImplementedException();
        }

        public void SplitAllButNCardsToNewStack(int n)
        {
            throw new NotImplementedException();
        }

        public bool AllowMerge()
        {
            return true;
        }

        public bool Retire(bool withVFX)
        {
            throw new NotImplementedException();
        }

        public bool Retire(string vfxName)
        {
            throw new NotImplementedException();
        }

        public void Decay(float interval)
        {
            throw new NotImplementedException();
        }

        public bool IsFront()
        {
            throw new NotImplementedException();
        }

        public bool CanAnimate()
        {
            throw new NotImplementedException();
        }

        public void StartArtAnimation()
        {
            throw new NotImplementedException();
        }

        public void FlipToFaceUp(bool instant)
        {
            throw new NotImplementedException();
        }

        public void FlipToFaceDown(bool instant)
        {
            throw new NotImplementedException();
        }

        public void Flip(bool state, bool instant)
        {
            throw new NotImplementedException();
        }

        public void ShowGlow(bool glowState, bool instant)
        {
            throw new NotImplementedException();
        }

        public Source StackSource { get; set; }
        public float LifetimeRemaining { get; set; }
        public Dictionary<string, string> GetCurrentIlluminations()
        {
            throw new NotImplementedException();
        }

        public void MoveTo<T>(T newLocation)
        {
            throw new NotImplementedException();
        }

        public void ModifyQuantity(int change)
        {
            Quantity = Quantity + change;
            if (Quantity <= 0)
            {
                Quantity = 0;
                Defunct = true;
            }

        }

        public bool AllowsMerge() {
            throw new NotImplementedException();
        }
    }
}
