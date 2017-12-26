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
    public class DeckEffectExecutorTests
    {
        private IGameEntityStorage _storage;
        private IDeckInstance _deckInstance;
        private const string ELEMENT_NAME_A = "Element_A";
        private const string DECK_NAME_A = "Deck_A";

        [SetUp]
        public void Setup()
        {
            _storage = Substitute.For<IGameEntityStorage>();
            _deckInstance = Substitute.For<IDeckInstance>();
            _storage.GetDeckInstanceById(DECK_NAME_A).Returns(_deckInstance);

        }

        public void DeckEffectExecutor_ReturnsElementId_ForValidElementId()
        {
            var ex=new DeckEffectExecutor(_storage);
            var ex=

        }
    }
}
