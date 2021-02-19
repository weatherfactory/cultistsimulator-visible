using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using SecretHistories.Abstract;
using SecretHistories.Commands;

namespace SubEncaustingTests
{

[TestFixture]
public class SubEncaustingTests
{


    public class OuterCommandX: IEncaustment
    {
        public InnerCommandX MarkedProperty { get; set; }
    }

    public class InnerCommandX: IEncaustment
    {
        public InmostCommandX MarkedProperty { get; set; }
    }

    public class InmostCommandX:IEncaustment
    {
        public string MarkedProperty { get; set; }

    }

    [IsEncaustableClass(typeof(OuterCommandX))]
    public class OuterEncaustableX : IEncaustable
    {
        [Encaust]
        public InnerEncaustableX MarkedProperty { get; set; }
    }

    [IsEncaustableClass(typeof(InnerCommandX))]
    public class InnerEncaustableX: IEncaustable
        {
            [Encaust]
            public InmostEncaustableX MarkedProperty { get; set; }
        }
    [IsEncaustableClass(typeof(InmostCommandX))]
    public class InmostEncaustableX: IEncaustable
    {
        [Encaust]
        public string MarkedProperty { get; set; }
    }

    [Test]
    public void EncaustableProperty_WhichIsItselfAnEncaustableObject_IsEncaustedToSubCommand()
    {
        var encaustery = new Encaustery<OuterCommandX>();
        var x1 = new OuterEncaustableX();
        var x2 = new InnerEncaustableX();
        var x3 = new InmostEncaustableX();
        x1.MarkedProperty = x2;
        x2.MarkedProperty = x3;

        var outerCommand = encaustery.Encaust(x1);
        Assert.IsInstanceOf<InnerCommandX>(outerCommand.MarkedProperty);
        Assert.IsInstanceOf<InmostCommandX>(outerCommand.MarkedProperty.MarkedProperty);

    }
    }
}
