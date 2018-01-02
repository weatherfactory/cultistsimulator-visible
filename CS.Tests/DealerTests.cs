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
        private IDeckInstance _subDeckInstance;
        private IDeckInstance _subSubDeckInstance;
        private const string DeckPrefix = "deck:";
        private const string ElementIdA = "Element_A";
        private const string DeckId = "Deck_TopDeck";
        private const string SubDeckId = "Deck_SubDeck";
        private const string SubSubDeckId = "Deck_SubSubDeck";

        private const string ElementDrawnFromSubDeckId = "Element_B";

        private const string ElementDrawnFromSubSubDeckId = "Element_C";


        [SetUp]
        public void Setup()
        {
            _storage = Substitute.For<IGameEntityStorage>();

            _deckInstance = Substitute.For<IDeckInstance>();
            _storage.GetDeckInstanceById(DeckId).Returns(_deckInstance);

            _subDeckInstance = Substitute.For<IDeckInstance>();
            _storage.GetDeckInstanceById(SubDeckId).Returns(_subDeckInstance);

            _subSubDeckInstance = Substitute.For<IDeckInstance>();
            _storage.GetDeckInstanceById(SubDeckId).Returns(_subSubDeckInstance);



        }
        [Test]
        public void Dealer_ReturnsElementId_ForValidElementId()
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
        public void Dealer_ReturnsDeckDraw_ForDeckSpecifiedId()
        {
            //set up the draw which returns deck: and a valid subdeck with that id
            var dealer = new Dealer(_storage);
            _deckInstance.Draw().Returns(DeckPrefix+SubDeckId);
            _subDeckInstance.Draw().Returns(ElementDrawnFromSubDeckId);

            var result = dealer.Deal(_deckInstance);
            Assert.AreEqual(ElementDrawnFromSubDeckId, result);


        }

        [Test]
        public void Dealer_ReturnsSubDeckDrawsRecursively()
        {
            var dealer=new Dealer(_storage);
            _deckInstance.Draw().Returns(DeckPrefix + SubDeckId);
            _subDeckInstance.Draw().Returns(DeckPrefix + SubSubDeckId);
            _subSubDeckInstance.Draw().Returns(ElementDrawnFromSubSubDeckId);

            var result = dealer.Deal(_deckInstance);

            Assert.AreEqual(ElementDrawnFromSubSubDeckId, result);

        }

    }
}

