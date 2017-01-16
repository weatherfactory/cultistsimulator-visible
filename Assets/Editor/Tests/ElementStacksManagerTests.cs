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
    public class ElementStacksManagerTests
    {
        private List<IElementStack> stacks;
        private ITokenTransformWrapper wrapper;
        [SetUp]
        public void Setup()
        {
          stacks= TestObjectGenerator.CardsForElements(TestObjectGenerator.ElementDictionary(1, 3));
            wrapper = Substitute.For<ITokenTransformWrapper>();
            wrapper.GetStacks().Returns(stacks);
        }

        [Test]
        public void ReduceElement_AffectsAspectPossessor_WhenNoConcreteElementFound()
        {
            var ecm = new ElementStacksManager(wrapper);
            FakeElementStack stackToRemove = stacks[0] as FakeElementStack;
            stackToRemove.Element.Aspects.Add("pureAspect",1);
            ecm.ReduceElement("pureAspect", -1);
            Assert.IsTrue(stackToRemove.Defunct);
        }

        [Test]
        public void Manager_SumsUniqueElements()
        {
            var ecg = new ElementStacksManager(wrapper);
            var d = ecg.GetCurrentElementTotals();
            foreach(var c in stacks)
            {
                Assert.AreEqual(1, d[c.Id]);
                
            }
        }

        [Test]
        public void Manager_SumsExtraNonUniqueElements()
        {
            stacks.Add(TestObjectGenerator.CreateElementCard(stacks[0].Id,1));
            var ecg = new ElementStacksManager(wrapper);
            var d = ecg.GetCurrentElementTotals();
             Assert.AreEqual(2,d[stacks[0].Id]);
            Assert.AreEqual(1, d[stacks[1].Id]);
            Assert.AreEqual(1, d[stacks[2].Id]);

        }


        [Test]
        public void Manager_SumsUniqueAspects()
        {
            var elements = TestObjectGenerator.ElementDictionary(1, 2);
            TestObjectGenerator.AddUniqueAspectsToEachElement(elements);
            var aspectedCards = TestObjectGenerator.CardsForElements(elements);
            wrapper.GetStacks().Returns(aspectedCards);

            var ecg = new ElementStacksManager( wrapper);
            var d = ecg.GetTotalAspects();
            Assert.AreEqual(1, d["1"]);
            Assert.AreEqual(1, d["a1"]);
            Assert.AreEqual(1, d["2"]);
            Assert.AreEqual(1, d["a2"]);
        }

        [Test]
        public void Manager_SumsDuplicateAspects()
        {
            var elements = TestObjectGenerator.ElementDictionary(1, 2);
            TestObjectGenerator.AddUniqueAspectsToEachElement(elements);
            elements["1"].Aspects.Add("a2",1);
            var aspectedCards = TestObjectGenerator.CardsForElements(elements);
            wrapper.GetStacks().Returns(aspectedCards);

            var ecg = new ElementStacksManager(wrapper);
            var d = ecg.GetTotalAspects();
            Assert.AreEqual(1, d["a1"]);
            Assert.AreEqual(2, d["a2"]);
        }
    

    [Test]
    public void Manager_ReduceElement_CanOnlyTakeNegativeArgument()
    {
                    var eca=new ElementStacksManager(wrapper);
            Assert.Throws<ArgumentException>(() => eca.ReduceElement("1", 0));
            Assert.Throws<ArgumentException>(() => eca.ReduceElement("1", 1));
    }

        [Test]
        public void Manager_ReduceElement_CallsRemoveOnSingleCard()
        {
            var eca = new ElementStacksManager(wrapper);
            FakeElementStack stackToRemove = stacks[0] as FakeElementStack;
            eca.ReduceElement(stackToRemove.Id,-1);
            Assert.IsTrue(stackToRemove.Defunct);
        }

        [Test]
        public void Manager_ReduceElementBy2_Removes2SingleCards()
        {
            var eca = new ElementStacksManager(wrapper);
            FakeElementStack firstStackToRemove = stacks[0] as FakeElementStack;
            FakeElementStack secondStackToRemove = stacks[1] as FakeElementStack;
            secondStackToRemove.Element = firstStackToRemove.Element;
                
            
            Assert.AreEqual(0,eca.ReduceElement(firstStackToRemove.Id, -2));
            Assert.IsTrue(firstStackToRemove.Defunct);
            Assert.IsTrue(secondStackToRemove.Defunct);
            //1 stack remaining out of 3
            Assert.AreEqual(1,stacks.Count(s => s.Defunct==false));
        }

        [Test]
        public void Manager_ReduceElementBy3_Removes2SingleCardsAndReturnsMinus1()
        {
            var eca = new ElementStacksManager(wrapper);
            FakeElementStack firstStackToRemove = stacks[0] as FakeElementStack;
            FakeElementStack secondStackToRemove = stacks[1] as FakeElementStack;
            secondStackToRemove.Element = firstStackToRemove.Element;


            Assert.AreEqual(-1,eca.ReduceElement(firstStackToRemove.Id, -3));
            Assert.IsTrue(firstStackToRemove.Defunct);
            Assert.IsTrue(secondStackToRemove.Defunct);

            //1 stack remaining out of 3
            Assert.AreEqual(1, stacks.Count(s => s.Defunct == false));          
        }

        [Test]
        public void Manager_IncreaseElement_CanOnlyTakePositiveArgument()
        {
            var ecg = new ElementStacksManager(wrapper);
            Assert.Throws<ArgumentException>(() => ecg.IncreaseElement("1", 0));
            Assert.Throws<ArgumentException>(() => ecg.IncreaseElement("1", -1));
        }

        [Test]
        public void Manager_IncreaseNewElementBy2_AddsNewStackOf2()
        {
            var ecg = new ElementStacksManager(wrapper);
            FakeElementStack newStack = TestObjectGenerator.CreateElementCard(stacks.Count + 1.ToString(), 2);
            wrapper.ProvisionElementStack(newStack.Id, newStack.Quantity).Returns(newStack);
            ecg.IncreaseElement(newStack.Id,newStack.Quantity);

            wrapper.Received().ProvisionElementStack(newStack.Id, newStack.Quantity);
        }

        [Test]
        public void Manager_AcceptsStack()
        {
            var ecg=new ElementStacksManager(wrapper);
            FakeElementStack newStack =TestObjectGenerator.CreateElementCard(stacks.Count+1.ToString(),2);
            ecg.AcceptStack(newStack);
            wrapper.Received().Accept(newStack);

        }


        
    }


    //I should mock this, actually
    public class FakeElementStack : IElementStack
    {
        public Element Element { get; set; }
        public string Id { get { return Element.Id; } }
        public string SaveLocationInfo { get; set; }
        public int Quantity { get; set; }
        public bool Defunct { get; private set; }
        public bool MarkedForConsumption { get; set; }

        public IAspectsDictionary GetAspects()
        {
            return Element.AspectsIncludingSelf;
        }

        public void SetQuantity(int quantity)
        {
            Quantity = quantity;
        }

        public void Populate(string elementId, int quantity)
        {
            Element = TestObjectGenerator.CreateElement(int.Parse(elementId));
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

        public void SplitAllButNCardsToNewStack(int n)
        {
            throw new NotImplementedException();
        }

        public bool AllowMerge()
        {
            return true;
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
