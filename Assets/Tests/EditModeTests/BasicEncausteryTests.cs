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
    
namespace BasicEncaustingTests
{
    [TestFixture]
    public class BasicEncausteryTests
    {
        [Test]
        public void Encausting_ReturnsInstanceOfSpecifiedCommandType_ForValidEncaustable()
        {
            var encaustery = new Encaustery<EncaustedCommandX>();
            var vex = new ValidEncaustableX();
            Assert.IsInstanceOf<EncaustedCommandX>(encaustery.Encaust(vex));
        }

        [Test]
        public void EncaustingThrowsException_WhenEncausteryPassedNonEncaustableClass()
        {
            var encaustery = new Encaustery<EncaustedCommandX>();
            var nex = new NonEncaustableX();
            Assert.Throws<ApplicationException>(() => encaustery.Encaust(nex));
        }

        [Test]
        public void EncaustingThrowsException_WhenGenericArgumentDoesntMatch_ToTypeForAttribute()
        {
            var encaustery = new Encaustery<SituationCreationCommand>();
            var vx = new ValidEncaustableX();
            Assert.Throws<ApplicationException>(() => encaustery.Encaust(vx));
        }


        [Test]
        public void EncaustingThrowsError_WhenPropertyEncaustmentStatusIsUnmarked()
        {
            var encaustery = new Encaustery<EncaustedCommandX>();
            var eupx = new EncaustableWithUnmarkedPropertyX();
            Assert.Throws<ApplicationException>(() => encaustery.Encaust(eupx));
        }

        [Test]
        public void EncaustingDoesntThrowError_WhenPropertyEncaustmentStatusInBaseClassIsUnmarked()
        {
            var encaustery = new Encaustery<EncaustedCommandX>();
            var dx = new DerivedEncaustableClassX();
            encaustery.Encaust(dx);
        }

        [Test]
        public void EncaustablePropertyValue_PopulatesMatchInIEncaustment()
        {
            var encaustery = new Encaustery<EncaustedCommandX>();
            var vx = new ValidEncaustableX { MarkedProperty = 1, OtherMarkedProperty = 2 };
            var encaustedCommand = encaustery.Encaust(vx);
            Assert.AreEqual(1, encaustedCommand.MarkedProperty);
            Assert.AreEqual(2, encaustedCommand.OtherMarkedProperty);
        }

        [Test]
        public void EncaustablePropertyWithoutCommandPropertyOfSameName_ThrowsApplicationException()
        {
            var encaustery = new Encaustery<EncaustedCommandX>();
            var ex = new EncaustableWithAPropertyThatCommandXDoesntHave { MarkedPropertyWithoutMatchInCommmand = 1 };
            Assert.Throws<ApplicationException>(() => encaustery.Encaust(ex));
        }

        [Test]
        public void CommandPropertyWithoutEncaustablePropertyOfSameName_ThrowsApplicationException()
        {
            var encaustery = new Encaustery<EncaustedCommandX>();
            var ex = new EncaustableMissingAPropertyThatCommandXHas();

            Assert.Throws<ApplicationException>(() => encaustery.Encaust(ex));
        }

        
    }

public class NonEncaustableX : IEncaustable
{ }

public class UsefulBaseClassThatIsntIntendedEncaustableX
{
    public int PropertyThatShouldntBeCheckedForEncaustAttributes { get; }
}

[IsEncaustableClass(typeof(EncaustedCommandX))]
public class DerivedEncaustableClassX: UsefulBaseClassThatIsntIntendedEncaustableX,IEncaustable
{
    [Encaust]
    public int MarkedProperty { get; set; }
    [Encaust]
    public int OtherMarkedProperty { get; set; }
    }

    [IsEncaustableClass(typeof(EncaustedCommandX))]
public class EncaustableWithUnmarkedPropertyX : IEncaustable
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
public class EncaustableWithAPropertyThatCommandXDoesntHave : IEncaustable
{
    [Encaust]
    public int MarkedProperty { get; set; }
    [Encaust]
    public int OtherMarkedProperty { get; set; }
    [Encaust]
    public int MarkedPropertyWithoutMatchInCommmand { get; set; }
}

[IsEncaustableClass(typeof(EncaustedCommandX))]
public class EncaustableMissingAPropertyThatCommandXHas : IEncaustable
{
    [Encaust]
    public int MarkedProperty { get; set; }
    [DontEncaust]
    public int OtherMarkedProperty { get; set; }
}


public class EncaustedCommandX
{
    public int MarkedProperty { get; set; }
    public int OtherMarkedProperty { get; set; }
}

}