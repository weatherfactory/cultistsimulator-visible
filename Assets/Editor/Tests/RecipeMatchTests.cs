using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

//IMPORTANT: requirements for these recipes determine if they can be triggered by the alternativerecipe property.
//They only take account of concrete resources, not their aspects.
//This is by design: the workspace is a place to choose to consume aspects. The whole mash of aspects in the players' resources
//would be all over the place (and would need multiplying on numbers of individual resources).

namespace CS.Tests
{
    public class TestElementContainer : IContainsElement
    {
        public Element Element { get; set; }

        //This isn't in use, now that I have a separate RecipeMatch class, but 
        //I'm keeping it around for the moment in case I make element storage/retrieval DI-based.
        public TestElementContainer(string id, string label, string description)
        {
            Element = new Element(id, label, description);
        }
    }


    public class RecipeMatchTests
    {
        private Dictionary<string, int> TestAspects;
        private const string COOLTH = "coolth";
        private const string WARMTH = "warmth";

        [SetUp]
        public void Setup()
        {

            TestAspects = new Dictionary<string, int>()
            {
                {COOLTH, 1},
                {WARMTH, 1}
            };



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
}