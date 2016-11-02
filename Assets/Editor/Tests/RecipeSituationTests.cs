using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;

namespace  CS.Tests
{
    

[TestFixture]
 public class RecipeSituationTests
{
    private Recipe r1;
    private Recipe r2;
    private Compendium rc;
    private IElementsContainer container;

     [SetUp]
    public void Setup()
        {
         r1 = new Recipe() {Id="r1",Warmup =1}; //NSubstitute doesn't like returning null from mock properties?
        r2 = new Recipe() { Id = "r2", Warmup = 2 }; //NSubstitute doesn't like returning null from mock properties?
            rc = new Compendium(new Dice());
            rc.UpdateRecipes(new List<Recipe>() { r1, r2 });
            container = Substitute.For<IElementsContainer>();
        }

        [Test]
        public void Recipe_WithoutLoopProperty_DoesNotRequestContinuation()
        {

            r1.Loop = null;
            RecipeSituation rs = new RecipeSituation(r1, 0, container, rc);


            rs.DoHeartbeat();

            Assert.IsNull(rs.CurrentRecipeId);
            Assert.AreEqual(RecipeTimerState.Extinct,rs.TimerState);
            Assert.AreEqual(0,rs.TimeRemaining);

        }

        [Test]
        public void Recipe_WithLoopProperty_RequestsContinuation()
        {

            r1.Loop = r2.Id;

            RecipeSituation rs = new RecipeSituation(r1, 0, container, rc);

            rs.DoHeartbeat();

            Assert.AreEqual(r2.Id, rs.CurrentRecipeId);
            Assert.AreEqual(RecipeTimerState.Ongoing, rs.TimerState);
            Assert.AreEqual(r2.Warmup, rs.TimeRemaining);

        }

    [Test]
    public void AlternativeRecipe_ReplacesOriginal_InRecipeSituation()
    {
        r1.Loop = r1.Id;
        r2.Loop = r2.Id;
            r1.AlternativeRecipes.Add(new RecipeAlternative(r2.Id,100,false));
            RecipeSituation rs = new RecipeSituation(r1, 0, container, rc);
            rs.DoHeartbeat();
            Assert.AreEqual(r2.Id,rs.CurrentRecipeId);
        }

    }




}