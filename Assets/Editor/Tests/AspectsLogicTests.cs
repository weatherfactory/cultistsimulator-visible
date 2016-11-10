using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Noon;
using NUnit.Framework;

namespace Assets.Editor.Tests
{
    [TestFixture]
    public class AspectsLogicTests
    {
        private Compendium c;
        private Dictionary<string, Element> elements;

        [SetUp]
        public void SetUp()
        {
            c = new Compendium(null);
            elements = TestObjectGenerator.ElementDictionary(1, 2);
            TestObjectGenerator.AddUniqueAspectsToEachElement(elements);
            c.UpdateElements(elements);
        }

        [Test]
        public void AspectsMatchFilter_ExcludesNonMatches()
        {
            string aspectId = elements[TestObjectGenerator.GeneratedElementId(1)].Aspects.Keys.Single();
            Dictionary<string, int> filter = new Dictionary<string, int>()
                {{aspectId, 1}};

            Dictionary<string, int> elementsToFilter = new Dictionary<string, int>();
            foreach (string k in elements.Keys)
            {
                elementsToFilter.Add(k, 10);
            }

            Dictionary<string, int> filteredElements = NoonUtility.AspectMatchFilter(filter, elementsToFilter, c);
            Assert.AreEqual(10, filteredElements[TestObjectGenerator.GeneratedElementId(1)]);
            Assert.IsFalse(filteredElements.ContainsKey(TestObjectGenerator.GeneratedElementId(2)));
        }

        [Test]
        public void AspectsMatchFilter_ExcludesInsufficientAspectValues()
        {
            string aspectId = elements[TestObjectGenerator.GeneratedElementId(1)].Aspects.Keys.Single();
            Dictionary<string, int> filter = new Dictionary<string, int>()
                {{aspectId, 2}}; //element 1 only has this aspect at 1

            Dictionary<string, int> elementsToFilter = new Dictionary<string, int>();
            foreach (string k in elements.Keys)
            {
                elementsToFilter.Add(k, 10);
            }

            Dictionary<string, int> filteredElements = NoonUtility.AspectMatchFilter(filter, elementsToFilter, c);
            Assert.IsFalse(filteredElements.ContainsKey(TestObjectGenerator.GeneratedElementId(1)));
            Assert.IsFalse(filteredElements.ContainsKey(TestObjectGenerator.GeneratedElementId(2)));
        }

        [Test]
        public void AspectsMatchFilter_UnderstandsElementsSpecifiedAsAspects()
        {
            Dictionary<string, int> filter = new Dictionary<string, int>()
                {{elements[TestObjectGenerator.GeneratedElementId(1)].Id, 1}};

            Dictionary<string, int> elementsToFilter = new Dictionary<string, int>();
            foreach (string k in elements.Keys)
            {
                elementsToFilter.Add(k, 10);
            }

            Dictionary<string, int> filteredElements = NoonUtility.AspectMatchFilter(filter, elementsToFilter, c);
            Assert.IsTrue(filteredElements.ContainsKey(TestObjectGenerator.GeneratedElementId(1)));
            Assert.IsFalse(filteredElements.ContainsKey(TestObjectGenerator.GeneratedElementId(2)));
        }
    }
}
