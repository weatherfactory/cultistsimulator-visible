using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SecretHistories.Commands;
using SecretHistories.UI;


    [TestFixture]
    public class CommandsEncaustOkay
    {

        [Test]
        public void ElementStack_Encausts()
        {
            var encaustery = new Encaustery<ElementStackCreationCommand>();
            var elementStack = new ElementStack();
            encaustery.Encaust(elementStack);
        }

        [Test]
        public void CharacterCreationCommand_Encausts()
        {
         var encaustery=new Encaustery<CharacterCreationCommand>();
         var character=new Character();
         encaustery.Encaust(character);
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
