using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

[TestFixture]
public class CharacterTests
{
    private const string EL1 = "el1";
    private const string EL2 = "el2";
    private const string EL3 = "el3";

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
        Assert.AreEqual(0, c.GetCurrentElementQuantityInStockpile(EL1));
        c.ModifyElementQuantity(EL1, 1);
        Assert.AreEqual(1, c.GetCurrentElementQuantityInStockpile(EL1));
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
        c.ElementOutOfStockpile(EL1, 3);
        Assert.AreEqual(3, c.GetCurrentElementQuantityInWorkspace(EL1));
        Assert.AreEqual(7, c.GetCurrentElementQuantityInStockpile(EL1));
        Assert.AreEqual(10, c.GetCurrentElementQuantity(EL1));

    }

    [Test]
    public void ElementFromWorkspace_IsReflectedInCounts()
    {
        Character c = new Character();
        c.ModifyElementQuantity(EL1, 10);
        c.ElementOutOfStockpile(EL1, 3);
        Assert.AreEqual(3, c.GetCurrentElementQuantityInWorkspace(EL1));
        Assert.AreEqual(7, c.GetCurrentElementQuantityInStockpile(EL1));
        Assert.AreEqual(10, c.GetCurrentElementQuantity(EL1));
        c.ElementIntoStockpile(EL1, 1);
        Assert.AreEqual(8, c.GetCurrentElementQuantityInStockpile(EL1));
        Assert.AreEqual(2, c.GetCurrentElementQuantityInWorkspace(EL1));
        Assert.AreEqual(10, c.GetCurrentElementQuantity(EL1));
    }

    [Test]
    public void ElementToWorkspace_FailsWithInsufficientElementQuantity()
    {
        Character c = new Character();
        Assert.IsFalse(c.ElementOutOfStockpile(EL1, 1));
        Assert.AreEqual(0, c.GetCurrentElementQuantityInWorkspace(EL1));
        c.ModifyElementQuantity(EL1, 1);
        Assert.IsFalse(c.ElementOutOfStockpile(EL1, 2));
        Assert.AreEqual(0, c.GetCurrentElementQuantityInWorkspace(EL1));
        c.ModifyElementQuantity(EL1, 1); //total element quantity should now be 2
        Assert.IsFalse(c.ElementOutOfStockpile(EL1, 3));
        Assert.AreEqual(0, c.GetCurrentElementQuantityInWorkspace(EL1));
    }

    [Test]
    public void ElementFromWorkspace_FailsWithInsuffientElementInWorkspaceQuantity()
    {
        Character c = new Character();
        c.ModifyElementQuantity(EL1, 5);
        Assert.IsFalse(c.ElementIntoStockpile(EL1, 1));
        Assert.AreEqual(5, c.GetCurrentElementQuantityInStockpile(EL1));
        c.ElementOutOfStockpile(EL1, 1); //1 total in workspace
        Assert.IsFalse(c.ElementIntoStockpile(EL1, 2));
        Assert.AreEqual(4, c.GetCurrentElementQuantityInStockpile(EL1));
        c.ElementOutOfStockpile(EL1, 1); //2 total in workspace
        Assert.IsFalse(c.ElementIntoStockpile(EL1, 3));
        Assert.AreEqual(3, c.GetCurrentElementQuantityInStockpile(EL1));
    }

    [Test]
    public void RemovingElement_AffectsWorkspaceQuantityLast()
    {
        Character c = new Character();
        c.ModifyElementQuantity(EL1, 5);
        c.ElementOutOfStockpile(EL1, 1);
        c.ModifyElementQuantity(EL1, -4);
        Assert.AreEqual(0, c.GetCurrentElementQuantityInStockpile(EL1));
        Assert.AreEqual(1, c.GetCurrentElementQuantityInWorkspace(EL1));
        Assert.AreEqual(1, c.GetCurrentElementQuantity(EL1));
    }

    [Test]
    public void RemovingElement_AffectsWorkspaceQuantity_OnceResourceQuantityConsumed()
    {
        Character c = new Character();
        c.ModifyElementQuantity(EL1, 5);
        c.ElementOutOfStockpile(EL1, 3); //2 left in resources, 3 in workspace, 5 total
        c.ModifyElementQuantity(EL1, -3); //should leave 0 in resources, 2 in workspace, 2 total
        Assert.AreEqual(0, c.GetCurrentElementQuantityInStockpile(EL1));
        Assert.AreEqual(2, c.GetCurrentElementQuantityInWorkspace(EL1));
        Assert.AreEqual(2, c.GetCurrentElementQuantity(EL1));
    }

    [Test]
    public void GetOutputElements_ReturnsElementsPossessedButNotInStockpile()
    {
        Character c = new Character();
        c.ModifyElementQuantity(EL1, 5);
        c.ModifyElementQuantity(EL2, 5);
        c.ModifyElementQuantity(EL3, 5);

        c.ElementOutOfStockpile(EL1, 5); //all in workspace
        c.ElementOutOfStockpile(EL2, 2); //2 into workspace
                                         //EL3 not moved into workspace

        Assert.AreEqual(5, c.GetOutputElements()[EL1]);
        Assert.AreEqual(2,c.GetOutputElements()[EL2]);
        Assert.IsFalse(c.GetOutputElements().ContainsKey(EL3));
    }
}

