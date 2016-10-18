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

            Hashtable slot1=new Hashtable()
            {
                {"forbidden", "foo1"},
                {"permitted", "bar1"}
            };
            Hashtable slot2 = new Hashtable()
            {
                {"forbidden", "foo2"},
                {"permitted", "bar2"}
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
        }
    }
}
