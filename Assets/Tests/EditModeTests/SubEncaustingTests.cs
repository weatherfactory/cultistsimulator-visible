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
    [IsEncaustableClass(typeof(OuterCommandX))]
    public class OuterEncaustableX : IEncaustable
    {
        [Encaust]
        public InnerEncaustableX MarkedProperty { get; set; }
    }

    [IsEncaustableClass(typeof(InnerCommandX))]
    public class InnerEncaustableX : IEncaustable
    {
        [Encaust]
        public InmostEncaustableX MarkedProperty { get; set; }
    }
    [IsEncaustableClass(typeof(InmostCommandX))]
    public class InmostEncaustableX : IEncaustable
    {
        [Encaust]
        public string MarkedProperty { get; set; }
    }

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


    [IsEncaustableClass(typeof(OuterCommandY))]
    public class OuterEncaustableY : IEncaustable
    {
        [Encaust]
        public List<InnerEncaustableY> MarkedListProperty { get; set; } = new List<InnerEncaustableY>();
    }

    [IsEncaustableClass(typeof(InnerCommandY))]
    public class InnerEncaustableY : IEncaustable
    {
        [Encaust]
        public string MarkedProperty { get; set; }
    }

        public class OuterCommandY : IEncaustment
    {
        public List<InnerCommandY> MarkedListProperty { get; set; }
    }

        public class InnerCommandY : IEncaustment
        {
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

    [Test]
    public void EncaustableProperty_WhichIsAListOfEncaustableObjects_IsEncaustedToSubCommand()
        {
        var encaustery = new Encaustery<OuterCommandY>();
        var y1=new OuterEncaustableY();
        var innery1=new InnerEncaustableY{MarkedProperty = "1"};
        var innery2 = new InnerEncaustableY { MarkedProperty = "2" };
            y1.MarkedListProperty.Add(innery1);
            y1.MarkedListProperty.Add(innery2);
            
        var outerCommand = encaustery.Encaust(y1);
        Assert.AreEqual("1",outerCommand.MarkedListProperty[0].MarkedProperty);
        Assert.AreEqual("2", outerCommand.MarkedListProperty[1].MarkedProperty);

        }

    }
}
