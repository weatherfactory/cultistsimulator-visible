using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SecretHistories.Commands;
using SecretHistories.UI;

namespace Assets.Tests.UnitTests
{
    [TestFixture]
    public class CheckingCommandsEncaustOkay
    {

        [Test]
        public void ElementStack_Encausts()
        {
            var encaustery=new Encaustery();
            var elementStack=new ElementStack();
            encaustery.EncaustTo<ElementStackCreationCommand>(elementStack);
        }

        [Test]
        public void CharacterCreationCommand_Encausts()
        {
            Assert.AreEqual(1, 0);
        }

        [Test]
        public void VerbToken_Encausts()
        {
            Assert.AreEqual(1, 0);
        }

        [Test]
        public void MiscToken_Encausts()
        {
            Assert.AreEqual(1, 0);
        }

        [Test]
        public void EncaustmentWorksOnDecks()
        {
            Assert.AreEqual(1, 0);
        }
    }
}
