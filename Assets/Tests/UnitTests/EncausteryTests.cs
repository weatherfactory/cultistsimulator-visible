using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application;
using SecretHistories.Abstract;

using NUnit.Framework;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using UnityEditorInternal.VR;

namespace Assets.Tests.UnitTests
{
    public class NonEncaustableX: IEncaustable
    { }



    [IsEncaustableClass(typeof(EncaustedCommandX))]
    public class EncaustableWithUnmarkedPropertyX: IEncaustable
    {
        [Encaust]
        public int MarkedProperty { get; }
        public int UnmarkedProperty { get; }
    }

    [IsEncaustableClass(typeof(EncaustedCommandX))]
    public class ValidEncaustableX : IEncaustable
    {
        [Encaust]
        public int MarkedProperty { get; }
        [DontEncaust]
        public int OtherMarkedProperty { get; }
        public int UnmarkedFIeldShouldBeOkay { get; set; }
    }

    public class EncaustedCommandX
    {

    }



    [TestFixture]
    public class EncausteryTests
    {
        [Test]
        public void Encausting_ReturnsInstanceOfSpecifiedCommandType_ForValidEncaustable()
        {
            var encaustery=new Encaustery();
            var vex=new ValidEncaustableX();
        Assert.IsInstanceOf<EncaustedCommandX>(encaustery.EncaustTo<EncaustedCommandX>(vex));
        }

        [Test]
        public void EncaustingThrowsException_WhenEncausteryPassedNonEncaustableClass()
        {
        var encaustery=new Encaustery();
        var nex = new NonEncaustableX();
        Assert.Throws(typeof(ApplicationException), () => encaustery.EncaustTo<EncaustedCommandX>(nex));
        }

        [Test]
        public void EncaustingThrowsException_WhenGenericArgumentDoesntMatch_ToTypeForAttribute()
        {
            var encaustery = new Encaustery();
            var eupx = new ValidEncaustableX();
            Assert.Throws(typeof(ApplicationException), () => encaustery.EncaustTo<SituationCreationCommand>(eupx));
        }


        [Test]
        public void EncaustingThrowsError_WhenPropertyEncaustmentStatusIsUnmarked()
        {
            Assert.AreEqual(1,0);
        }
        [Test]
        public void EncaustablePropertyValue_PopulatesMatchInIEncaustment()
        {
            Assert.AreEqual(1, 0);
        }

        [Test]
        public void NonEncaustablePropertyValue_DoesntPopulateMatchInIEncaustment()
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
