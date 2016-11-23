using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
    public class ElementStacksGatewayTests
    {
        private List<IElementStack> stacks;
        private IElementStacksWrapper wrapper;
        [SetUp]
        public void Setup()
        {
          stacks= TestObjectGenerator.CardsForElements(TestObjectGenerator.ElementDictionary(1, 3));
            wrapper = Substitute.For<IElementStacksWrapper>();
            wrapper.GetStacks().Returns(stacks);
        }

        [Test]
        public void Gateway_SumsUniqueElements()
        {
            var ecg = new ElementStacksGateway(wrapper);
            var d = ecg.GetCurrentElementTotals();
            foreach(var c in stacks)
            {
                Assert.AreEqual(1, d[c.ElementId]);
                
            }
        }

        [Test]
        public void Gateway_SumsExtraNonUniqueElements()
        {
            stacks.Add(TestObjectGenerator.CreateElementCard(stacks[0].ElementId,1));
            var ecg = new ElementStacksGateway(wrapper);
            var d = ecg.GetCurrentElementTotals();
             Assert.AreEqual(2,d[stacks[0].ElementId]);
            Assert.AreEqual(1, d[stacks[1].ElementId]);
            Assert.AreEqual(1, d[stacks[2].ElementId]);

        }


        [Test]
        public void Gateway_SumsUniqueAspects()
        {
            var elements = TestObjectGenerator.ElementDictionary(1, 2);
            TestObjectGenerator.AddUniqueAspectsToEachElement(elements);
            var aspectedCards = TestObjectGenerator.CardsForElements(elements);
            wrapper.GetStacks().Returns(aspectedCards);

            var ecg = new ElementStacksGateway( wrapper);
            var d = ecg.GetTotalAspects();
            Assert.AreEqual(1, d["1"]);
            Assert.AreEqual(1, d["a1"]);
            Assert.AreEqual(1, d["2"]);
            Assert.AreEqual(1, d["a2"]);
        }

        [Test]
        public void Gateway_SumsDuplicateAspects()
        {
            var elements = TestObjectGenerator.ElementDictionary(1, 2);
            TestObjectGenerator.AddUniqueAspectsToEachElement(elements);
            elements["1"].Aspects.Add("a2",1);
            var aspectedCards = TestObjectGenerator.CardsForElements(elements);
            wrapper.GetStacks().Returns(aspectedCards);


            var ecg = new ElementStacksGateway(wrapper);
            var d = ecg.GetTotalAspects();
            Assert.AreEqual(1, d["a1"]);
            Assert.AreEqual(2, d["a2"]);
        }
    

    [Test]
    public void Gateway_ReduceElement_CanOnlyTakeNegativeArgument()
    {
    var eca=new ElementStacksGateway(wrapper);
            Assert.Throws<ArgumentException>(() => eca.ReduceElement("1", 0));
            Assert.Throws<ArgumentException>(() => eca.ReduceElement("1", 1));
    }

        [Test]
        public void Gateway_ReduceElement_CallsRemoveOnSingleCard()
        {
            var eca = new ElementStacksGateway(wrapper);
            FakeElementStack stackToRemove = stacks[0] as FakeElementStack;
            eca.ReduceElement(stackToRemove.ElementId,-1);
            Assert.IsTrue(stackToRemove.Defunct);
        }

        [Test]
        public void Gateway_ReduceElementBy2_Removes2SingleCards()
        {
            var eca = new ElementStacksGateway(wrapper);
            FakeElementStack firstStackToRemove = stacks[0] as FakeElementStack;
            FakeElementStack secondStackToRemove = new FakeElementStack()
            {
                ElementId = firstStackToRemove.ElementId,
                Quantity = 1
            };
            stacks.Add(secondStackToRemove);
            
            Assert.AreEqual(0,eca.ReduceElement(firstStackToRemove.ElementId, -2));
            Assert.IsTrue(firstStackToRemove.Defunct);
            Assert.IsTrue(secondStackToRemove.Defunct);
            //2 stacks remaining out of 3
            Assert.AreEqual(2,stacks.Count(s => s.Defunct==false));
        }

        [Test]
        public void Gateway_ReduceElementBy3_Removes2SingleCardsAndReturnsMinus1()
        {
            var eca = new ElementStacksGateway(wrapper);
            FakeElementStack firstStackToRemove = stacks[0] as FakeElementStack;
            FakeElementStack secondStackToRemove = new FakeElementStack()
            {
                ElementId = firstStackToRemove.ElementId,
                Quantity = 1
            };
            stacks.Add(secondStackToRemove);
            

            Assert.AreEqual(-1,eca.ReduceElement(firstStackToRemove.ElementId, -3));
            Assert.IsTrue(firstStackToRemove.Defunct);
            Assert.IsTrue(secondStackToRemove.Defunct);

            //2 stacks remaining out of 3
            Assert.AreEqual(2, stacks.Count(s => s.Defunct == false));          
        }

        [Test]
        public void Gateway_IncreaseElement_CanOnlyTakePositiveArgument()
        {
            var ecg = new ElementStacksGateway(wrapper);
            Assert.Throws<ArgumentException>(() => ecg.IncreaseElement("1", 0));
            Assert.Throws<ArgumentException>(() => ecg.IncreaseElement("1", -1));
        }

        [Test]
        public void Gateway_IncreaseNewElementBy2_AddsNewStackOf2()
        {
            var ecg = new ElementStacksGateway(wrapper);
            FakeElementStack newStack = new FakeElementStack() {ElementId ="newElement",Quantity = 2};

            wrapper.ProvisionElementStack(newStack.ElementId, newStack.Quantity).Returns(newStack);
            ecg.IncreaseElement(newStack.ElementId,newStack.Quantity);

            wrapper.Received().ProvisionElementStack(newStack.ElementId, newStack.Quantity);
        }

        [Test]
        public void Gateway_AcceptsStack()
        {
            var ecg=new ElementStacksGateway(wrapper);
            FakeElementStack newStack = new FakeElementStack() { ElementId = "newElement", Quantity = 2 };
            ecg.AcceptStack(newStack);
            wrapper.Received().Accept(newStack);

        }


        
    }


    //I should mock this, actually
    public class FakeElementStack : IElementStack
    {
        public IAspectsDictionary Aspects;
        public string ElementId { get; set; }
        public int Quantity { get; set; }
        public bool Defunct { get; private set; }

        public IAspectsDictionary GetAspects()
        {
            return Aspects;
        }

        public void SetQuantity(int quantity)
        {
            Quantity = quantity;
        }

        public void Populate(string elementId, int quantity)
        {
            ElementId = elementId;
            Quantity = quantity;
        }

        public List<SlotSpecification> GetChildSlotSpecifications()
        {
            throw new NotImplementedException();
        }

        public bool HasChildSlots()
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
    }
}
