using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using NUnit.Framework;

namespace CS.Tests
{
    public class ElementTests
    {
        public Element Element;

        [SetUp]
        public void Setup()
        {
            Element = new Element("", "", "");
        }

        [Test]
        public void ElementWithNoSlotsPopulatesSafely()
        {

            Assert.AreEqual(0, Element.ChildSlots.Count);
            Element.AddSlotsFromHashtable(null);
            Assert.AreEqual(0, Element.ChildSlots.Count);
        }

        [Test]
        public void ElementSlotsArePopulated()
        {

            Assert.AreEqual(0, Element.ChildSlots.Count);
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

            Assert.AreEqual(2, Element.ChildSlots.Count);
            Assert.AreEqual(SLOT_LABEL_1,Element.ChildSlots[0].Label);
            Assert.AreEqual(SLOT_LABEL_2, Element.ChildSlots[1].Label);

            Assert.AreEqual(1, Element.ChildSlots[0].Required["slot1ra"]);
            Assert.AreEqual(1, Element.ChildSlots[0].Required.Count);
            Assert.AreEqual(0,Element.ChildSlots[0].Forbidden.Count);

            Assert.AreEqual(0, Element.ChildSlots[1].Required.Count);
            Assert.AreEqual(2, Element.ChildSlots[1].Forbidden.Count);
            Assert.AreEqual(1, Element.ChildSlots[1].Forbidden["slot2fa"]);
            Assert.AreEqual(1, Element.ChildSlots[1].Forbidden["slot2fb"]);


        }
    }
}
