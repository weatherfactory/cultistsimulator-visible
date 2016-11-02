using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using NUnit.Framework;

namespace CS.Tests
{
    [TestFixture]
    public class ElementTests
    {
        public Element Element;

        [SetUp]
        public void Setup()
        {
            Element = TestObjectGenerator.CreateElement(1);
        }

        [Test]
        public void ElementWithNoSlotsPopulatesSafely()
        {

            Assert.AreEqual(0, Element.ChildSlotSpecifications.Count);
            Element.AddSlotsFromHashtable(null);
            Assert.AreEqual(0, Element.ChildSlotSpecifications.Count);
        }

        [Test]
        public void ElementPossessingOneRequiredAspectFulfilsChildSlotSpecification()
        {
            Element.Aspects.Add("notarequiredaspect", 1);
            Element.Aspects.Add("requiredaspect", 1);

            ChildSlotSpecification css = new ChildSlotSpecification("specificationtotest");
            css.Required.Add("requiredaspect", 1);
            css.Required.Add("otherrequiredaspect", 1);
            ElementSlotMatch esm = css.GetElementSlotMatchFor(Element);
            Assert.AreEqual(ElementSlotSuitability.Okay, esm.ElementSlotSuitability);
        }

        [Test]
        public void ElementWithAnyAspect_Fulfils_ChildSlotSpecification_WithOneAspect()
        {
            Element.Aspects.Add("someaspect",1);
            ChildSlotSpecification css = new ChildSlotSpecification("specificationtotest");
            ElementSlotMatch esm = css.GetElementSlotMatchFor(Element);
            Assert.AreEqual(ElementSlotSuitability.Okay,esm.ElementSlotSuitability);
        }

        [Test]
        public void Element_AspectsIncludingSelf_IncludesSelf()
        {
            Element.Aspects.Add("actualaspect",1);
            Assert.AreEqual(1,Element.AspectsIncludingSelf[Element.Id]);
            Assert.AreEqual(1, Element.AspectsIncludingSelf["actualaspect"]);
        }

        [Test]
        public void ElementWithMissingAllRequiredAspectsFailsChildSlotSpecification()
        {
            Element.Aspects.Add("notarequiredaspect",1);

            ChildSlotSpecification css=new ChildSlotSpecification("specificationtotest");
            css.Required.Add("requiredaspect",1);
            css.Required.Add("otherrequiredaspect", 1);
            ElementSlotMatch esm=css.GetElementSlotMatchFor(Element);
            Assert.AreEqual(ElementSlotSuitability.RequiredAspectMissing,esm.ElementSlotSuitability);
            Assert.AreEqual("requiredaspect",esm.ProblemAspectIds.First());
            Assert.AreEqual("otherrequiredaspect", esm.ProblemAspectIds.Last());
        }

        [Test]
        public void ElementPossessingForbiddenAspectFailsChildSlotSpecification()
        {
            Element.Aspects.Add("forbiddenaspect", 1);

            ChildSlotSpecification css = new ChildSlotSpecification("specificationtotest");
            css.Forbidden.Add("forbiddenaspect", 1);
            ElementSlotMatch esm = css.GetElementSlotMatchFor(Element);
            Assert.AreEqual(ElementSlotSuitability.ForbiddenAspectPresent, esm.ElementSlotSuitability);
            Assert.AreEqual("forbiddenaspect", esm.ProblemAspectIds.First());
        }

        [Test]
        public void ElementSlotsArePopulated()
        {

            Assert.AreEqual(0, Element.ChildSlotSpecifications.Count);
            const string SLOT_LABEL_1 = "slotlabel1";
            const string SLOT_LABEL_2 = "slotlabel2";

            Hashtable slot1Required = new Hashtable() //1 thing required
            {
                {"slot1ra", 1}
            };
            Hashtable slot1Forbidden = new Hashtable(); //nothing is forbidden

            Hashtable slot1 = new Hashtable()
            {
                {"forbidden", slot1Forbidden},
                {"required", slot1Required}
            };

            Hashtable slot2Required = new Hashtable(); //nothing is required
            Hashtable slot2Forbidden = new Hashtable() //two things forbidden
            {  {"slot2fa", 1},
                {"slot2fb", 1}
            };


            Hashtable slot2 = new Hashtable()
            {
                {"required", slot2Required},
                { "forbidden", slot2Forbidden}
            };
            Hashtable ht = new Hashtable()
            {
                {SLOT_LABEL_1,slot1},
                {SLOT_LABEL_2,slot2},
            };


            Element.AddSlotsFromHashtable(ht);

            Assert.AreEqual(2, Element.ChildSlotSpecifications.Count);
            Assert.AreEqual(SLOT_LABEL_1,Element.ChildSlotSpecifications[0].Label);
            Assert.AreEqual(SLOT_LABEL_2, Element.ChildSlotSpecifications[1].Label);

            Assert.AreEqual(1, Element.ChildSlotSpecifications[0].Required["slot1ra"]);
            Assert.AreEqual(1, Element.ChildSlotSpecifications[0].Required.Count);
            Assert.AreEqual(0,Element.ChildSlotSpecifications[0].Forbidden.Count);

            Assert.AreEqual(0, Element.ChildSlotSpecifications[1].Required.Count);
            Assert.AreEqual(2, Element.ChildSlotSpecifications[1].Forbidden.Count);
            Assert.AreEqual(1, Element.ChildSlotSpecifications[1].Forbidden["slot2fa"]);
            Assert.AreEqual(1, Element.ChildSlotSpecifications[1].Forbidden["slot2fb"]);


        }
    }
}
