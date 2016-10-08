using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

public class TestElementContainer:IContainsElement
{
    public Element Element { get; set; }

    public TestElementContainer(string id,string label,string description)
    {
        Element=new Element(id,label,description);
    }
}


public class CurrentAspectsTests
{
    private CurrentAspects currentAspects;
    private TestElementContainer fireContainer;
    private TestElementContainer waterContainer;
    private TestElementContainer[] elementContainers;
    private const string COOLTH = "coolth";
    private const string WARMTH = "warmth";
    [SetUp]
        public void Setup()
        {
        currentAspects = new CurrentAspects();

        fireContainer =new TestElementContainer("fire", "fire", "fire");
        waterContainer = new TestElementContainer("water", "water", "water");
        fireContainer.Element.Aspects.Add(WARMTH,1);
        waterContainer.Element.Aspects.Add(COOLTH, 2);

        elementContainers=new TestElementContainer[2];
            elementContainers[0] = fireContainer;
        elementContainers[1] = waterContainer;


    }

        [Test]
        public void CurrentAspectsReturnsPopulatedAspects()
        {
            currentAspects.UpdateAspects(elementContainers);
        }

    [Test]
        public void NoAspectsPresentMatchesNoRecipe()
        {
            throw new NotImplementedException();
        }

    [Test]
    public void OneAspectPresentMatchesHighestPriorityRecipe()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void TwoAspectsPresentMatchesHighestPriority()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void TwoAspectsPresentOneAtLevelTwoMatchesHighestPriority()
    {
        throw new NotImplementedException();
    }
}
