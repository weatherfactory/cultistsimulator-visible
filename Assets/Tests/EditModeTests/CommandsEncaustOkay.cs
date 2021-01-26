using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using NUnit.Framework;
using SecretHistories.Commands;
using SecretHistories.UI;
using UnityEngine;
using Object = UnityEngine.Object;


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
         var characterObject = new GameObject();
         characterObject.AddComponent<Character>();
        characterObject.GetComponent<Character>().ActiveLegacy=new NullLegacy();
        characterObject.GetComponent<Character>().EndingTriggered=NullEnding.Create();

        encaustery.Encaust(characterObject.GetComponent<Character>());
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
