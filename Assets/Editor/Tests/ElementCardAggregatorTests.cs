using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
    public class ElementCardAggregatorTests
    {
        private List<IElementCard> cards;

            [SetUp]
        public void Setup()
        {
          cards= TestObjectGenerator.CardsForElements(TestObjectGenerator.ElementDictionary(1, 3));

        }

        [Test]
        public void Aggregator_SumsUniqueElements()
        {
            var ecg = new ElementCardsAggregator(cards);
            var d = ecg.GetCurrentElementTotals();
            foreach(var c in cards)
            {
                Assert.AreEqual(1, d[c.ElementId]);
                
            }
        }

        [Test]
        public void Aggregator_SumsExtraNonUniqueElements()
        {
            cards.Add(TestObjectGenerator.CreateElementCard(cards[0].ElementId,1));
            var ecg = new ElementCardsAggregator(cards);
            var d = ecg.GetCurrentElementTotals();
             Assert.AreEqual(2,d[cards[0].ElementId]);
            Assert.AreEqual(1, d[cards[1].ElementId]);
            Assert.AreEqual(1, d[cards[2].ElementId]);

        }


        [Test]
        public void Aggregator_SumsUniqueAspects()
        {
            var elements = TestObjectGenerator.ElementDictionary(1, 2);
            TestObjectGenerator.AddUniqueAspectsToEachElement(elements);
            var aspectedCards = TestObjectGenerator.CardsForElements(elements);

            var ecg = new ElementCardsAggregator(aspectedCards);
            var d = ecg.GetTotalAspects();
            Assert.AreEqual(1, d["1"]);
            Assert.AreEqual(1, d["a1"]);
            Assert.AreEqual(1, d["2"]);
            Assert.AreEqual(1, d["a2"]);
        }

        [Test]
        public void Aggregator_SumsDuplicateAspects()
        {
            var elements = TestObjectGenerator.ElementDictionary(1, 2);
            TestObjectGenerator.AddUniqueAspectsToEachElement(elements);
            elements["1"].Aspects.Add("a2",1);
            var aspectedCards = TestObjectGenerator.CardsForElements(elements);

            var ecg = new ElementCardsAggregator(aspectedCards);
            var d = ecg.GetTotalAspects();
            Assert.AreEqual(1, d["a1"]);
            Assert.AreEqual(2, d["a2"]);
        }

    }

    public class FakeElementCard : IElementCard
    {
        public Dictionary<string, int> Aspects;
        public string ElementId { get; set; }
        public int Quantity { get; set; }
        public Dictionary<string, int> GetAspects()
        {
            return Aspects;
        }
    }
}
