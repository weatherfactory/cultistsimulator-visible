using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application;
using Assets.Scripts.Application.Abstract;
using NUnit.Framework;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using UnityEditorInternal.VR;

namespace Assets.Tests.UnitTests
{
    public class NonEncaustableDummy: IEncaustable
    { }



    [EncaustableClass(typeof(EncaustedCommandDummy))]
    public class EncaustableWithUnmarkedPropertyDummy: IEncaustable
    {
        [Encaust]
        public int MarkedProperty { get; }
        public int UnmarkedProperty { get; }
    }

    public class EncaustedCommandDummy
    {

    }




    [TestFixture]
    public class EncausteryTests
    {
        [Test]
        public void EncaustingThrowsException_WhenEncausteryPassedNonEncaustableClass()
        {
        var encaustery=new Encaustery();
        var dec = new NonEncaustableDummy();
        Assert.Throws(typeof(ApplicationException), () => encaustery.EncaustTo<EncaustedCommandDummy>(dec));
        }

        [Test]
        public void EncaustingThrowsException_WhenGenericArgumentDoesntMatch_ToTypeForAttribute()
        {
            var encaustery = new Encaustery();
            var dec = new NonEncaustableDummy();
            Assert.Throws(typeof(ApplicationException), () => encaustery.EncaustTo<SituationCreationCommand>(dec));
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
