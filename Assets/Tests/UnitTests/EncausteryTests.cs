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
        public int MarkedProperty { get; set; }
        public int UnmarkedProperty { get; set; }
    }

    [IsEncaustableClass(typeof(EncaustedCommandX))]
    public class ValidEncaustableX : IEncaustable
    {
        [Encaust]
        public int MarkedProperty { get; set; }
        [Encaust]
        public int OtherMarkedProperty { get; set; }
        [DontEncaust]
        public int MarkedAsDontEncaustProperty { get; set; }

        public int UnmarkedFIeldShouldBeOkay;
    }

    [IsEncaustableClass(typeof(EncaustedCommandX))]
    public class EncaustableWithPropertyMissingFromCommandX:IEncaustable
    {
        [Encaust]
        public int MarkedPropertyWithoutMatchInCommmand { get; set; }
    }

    public class EncaustedCommandX
    {
        public int MarkedProperty { get; set; }
        public int OtherMarkedProperty { get; set; }
        public int MarkedAsDontEncaustProperty { get; set; }

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
        Assert.Throws<ApplicationException>( () => encaustery.EncaustTo<EncaustedCommandX>(nex));
        }

        [Test]
        public void EncaustingThrowsException_WhenGenericArgumentDoesntMatch_ToTypeForAttribute()
        {
            var encaustery = new Encaustery();
            var vx = new ValidEncaustableX();
            Assert.Throws<ApplicationException>( () => encaustery.EncaustTo<SituationCreationCommand>(vx));
        }


        [Test]
        public void EncaustingThrowsError_WhenPropertyEncaustmentStatusIsUnmarked()
        {
            var encaustery=new Encaustery();
            var eupx=new EncaustableWithUnmarkedPropertyX();
            Assert.Throws<ApplicationException>(() => encaustery.EncaustTo<EncaustedCommandX>(eupx));
        }
        [Test]
        public void EncaustablePropertyValue_PopulatesMatchInIEncaustment()
        {
         var encaustery=new Encaustery();
         var vx = new ValidEncaustableX {MarkedProperty = 1, OtherMarkedProperty = 2};
         var encaustedCommand= encaustery.EncaustTo<EncaustedCommandX>(vx);
         Assert.AreEqual(1,encaustedCommand.MarkedProperty);
         Assert.AreEqual(2,encaustedCommand.OtherMarkedProperty);
        }

        [Test]
        public void NonEncaustablePropertyValue_DoesntPopulateMatchInIEncaustment()
        {
            var encaustery = new Encaustery();
            var vx = new ValidEncaustableX { MarkedAsDontEncaustProperty = 3 };
            var encaustedCommand = encaustery.EncaustTo<EncaustedCommandX>(vx);
            Assert.AreEqual(0,encaustedCommand.MarkedAsDontEncaustProperty);
        }


        [Test]
        public void EncaustablePropertyWithoutCommandPropertyOfSameName_ThrowsApplicationException()
        {
            var encaustery=new Encaustery();
            var ewpm=new EncaustableWithPropertyMissingFromCommandX{MarkedPropertyWithoutMatchInCommmand = 1};
            Assert.Throws<ApplicationException>(() => encaustery.EncaustTo<EncaustedCommandX>(ewpm));
        }

   


    }
}
