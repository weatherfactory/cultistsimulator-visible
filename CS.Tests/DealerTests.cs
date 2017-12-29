using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.Logic;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CS.Tests
{
    [TestFixture]
    public class DealerTests
    {
        private IGameEntityStorage _storage;
        private IDeckInstance _deckInstance;
        private const string DeckPrefix = "deck:";
        private const string ElementIdA = "Element_A";
        private const string DeckId = "Deck_TopDeck";
        private const string SubDeckId = "Deck_SubDeck";
   
        private const string ElementDrawnFromSubDeckId = "Element_B";

        [SetUp]
        public void Setup()
        {
            _storage = Substitute.For<IGameEntityStorage>();
            _deckInstance = Substitute.For<IDeckInstance>();
            _storage.GetDeckInstanceById(SubDeckId).Returns(_deckInstance);
            
        }
        [Test]
        public void DeckEffectExecutor_ReturnsElementId_ForValidElementId()
        {
            var dealer=new Dealer(_storage);
            _deckInstance.Draw().Returns(ElementIdA);
            var result=dealer.Deal(_deckInstance);
            Assert.AreEqual(ElementIdA,result);
        }

        /// <summary>
        /// If the entry is specified with deck:, return a draw from that deck
        /// </summary>
        [Test]
        public void DeckEffectExecutor_ReturnsDeckDraw_ForDeckSpecifiedId()
        {
            var dealer = new Dealer(_storage);
            _deckInstance.Draw().Returns(DeckPrefix+SubDeckId);
            var subDeckInstance = Substitute.For<DeckInstance>();

            _storage.GetDeckInstanceById(SubDeckId).Returns(subDeckInstance);
            var result = dealer.Deal(_deckInstance);
            Assert.AreEqual(ElementDrawnFromSubDeckId, result);
        }


    }
}
