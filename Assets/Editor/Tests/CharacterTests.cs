using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

[TestFixture]
public class CharacterTests
{
    private const string EL1 = "el1";

    [Test]
    public void AddingElement_AffectsBasicCount()
    {
        Character c = new Character();
        Assert.AreEqual(0, c.GetCurrentElementQuantity(EL1));
        c.ModifyElementQuantity(EL1, 1);
        Assert.AreEqual(1, c.GetCurrentElementQuantity(EL1));
    }

    [Test]
    public void AddingElement_AffectsResourcesCount()
    {
        Character c = new Character();
        Assert.AreEqual(0, c.GetCurrentElementQuantityInResources(EL1));
        c.ModifyElementQuantity(EL1, 1);
        Assert.AreEqual(1, c.GetCurrentElementQuantityInResources(EL1));
    }

    [Test]
    public void AddingElement_DoesNotAffectWorkspaceCount()
    {
        Character c = new Character();
        Assert.AreEqual(0, c.GetCurrentElementQuantityInWorkspace(EL1));
        c.ModifyElementQuantity(EL1, 1);
        Assert.AreEqual(0, c.GetCurrentElementQuantityInWorkspace(EL1));
    }

    [Test]
    public void ElementToWorkspace_IsReflectedInCounts()
    {
        Character c = new Character();
        c.ModifyElementQuantity(EL1, 10);
        c.ElementToWorkspace(EL1, 3);
        Assert.AreEqual(3, c.GetCurrentElementQuantityInWorkspace(EL1));
        Assert.AreEqual(7, c.GetCurrentElementQuantityInResources(EL1));
        Assert.AreEqual(10, c.GetCurrentElementQuantity(EL1));
 
    }

    [Test]
    public void ElementFromWorkspace_IsReflectedInCounts()
    {
        Character c = new Character();
        c.ModifyElementQuantity(EL1,10);
        c.ElementToWorkspace(EL1, 3);
        Assert.AreEqual(3, c.GetCurrentElementQuantityInWorkspace(EL1));
        Assert.AreEqual(7, c.GetCurrentElementQuantityInResources(EL1));
        Assert.AreEqual(10, c.GetCurrentElementQuantity(EL1));
        c.ElementFromWorkspace(EL1, 1);
        Assert.AreEqual(8, c.GetCurrentElementQuantityInResources(EL1));
        Assert.AreEqual(2, c.GetCurrentElementQuantityInWorkspace(EL1));
        Assert.AreEqual(10, c.GetCurrentElementQuantity(EL1));
    }

    [Test]
    public void ElementToWorkspace_FailsWithInsufficientElementQuantity()
    {
        Character c = new Character();
       Assert.IsFalse(c.ElementToWorkspace(EL1, 1));
        Assert.AreEqual(0, c.GetCurrentElementQuantityInWorkspace(EL1));
        c.ModifyElementQuantity(EL1,1);
        Assert.IsFalse(c.ElementToWorkspace(EL1, 2));
        Assert.AreEqual(0, c.GetCurrentElementQuantityInWorkspace(EL1));
        c.ModifyElementQuantity(EL1, 1); //total element quantity should now be 2
        Assert.IsFalse(c.ElementToWorkspace(EL1, 3));
        Assert.AreEqual(0, c.GetCurrentElementQuantityInWorkspace(EL1));
    }

    [Test]
    public void ElementFromWorkspace_FailsWithInsuffientElementInWorkspaceQuantity()
    {
        Character c = new Character();
        c.ModifyElementQuantity(EL1, 5);
        Assert.IsFalse(c.ElementFromWorkspace(EL1, 1));
        Assert.AreEqual(5, c.GetCurrentElementQuantityInResources(EL1));
        c.ElementToWorkspace(EL1, 1); //1 total in workspace
        Assert.IsFalse(c.ElementFromWorkspace(EL1, 2));
        Assert.AreEqual(4, c.GetCurrentElementQuantityInResources(EL1));
        c.ElementToWorkspace(EL1, 1); //2 total in workspace
        Assert.IsFalse(c.ElementFromWorkspace(EL1, 3));
        Assert.AreEqual(3, c.GetCurrentElementQuantityInResources(EL1));
    }
}

