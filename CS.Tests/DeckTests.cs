using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using NUnit.Framework;

namespace CS.Tests
{
    public class DeckTests
    {
        private DeckInstance testDeck;
        private DeckSpec testDeckSpec;
        private const string A_CARD = "A_CARD";
        private const string DEFAULT_DRAW = "defaultdraw";
        [SetUp]
        public void Setup()
        {
          
            testDeckSpec=new DeckSpec("test",new List<string>(), DEFAULT_DRAW,false);
            testDeck=new DeckInstance(testDeckSpec);
        }

        [Test]
        public void DrawFromDeckDrawsTopCard()
        {
            testDeck.Add(A_CARD);
            var drawnCard = testDeck.Draw();
            Assert.AreEqual(A_CARD,drawnCard);
        }
        [Test]
        public void DrawFromEmptyNonResettingDeckDrawsDefaultCard()
        {
            var drawnCard = testDeck.Draw();
            Assert.AreEqual(DEFAULT_DRAW, drawnCard);
        }
        [Test]
        public void DrawFromEmptyNonResettingDeckDrawsOriginalCard()
        {
            testDeckSpec.StartingCards.Add(A_CARD);
            testDeckSpec.ResetOnExhaustion = true;
            Assert.AreEqual(0,testDeck.GetCurrentCardsAsList().Count);
            var drawnCard = testDeck.Draw();
            Assert.AreEqual(A_CARD, drawnCard);
        }
    }
}
