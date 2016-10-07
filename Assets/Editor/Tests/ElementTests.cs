using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;


   public class ElementTests
   {
       public Element Element;

        [SetUp]
        public void Setup()
        {
            Element=new Element("","","");
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
        Hashtable ht=new Hashtable()
        {
            {"quantity","3" },
            {"forbidden","foo" },
            {"permitted","bar" }
        };
        Element.AddSlotsFromHashtable(ht);
        Assert.AreEqual(3, Element.ChildSlots.Count);
        }
    }
