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

    [TestFixture]
    public class RecipeMatchTests
    {
       
        private const string COOLTH = "coolth";
        private const string WARMTH = "warmth";
        private const int COOLTH_VALUE = 5;
        private const int WARMTH_VALUE = 5;
        private const string MATCHING_VERB = "matchingverb";
        private const string ODD_VERB = "oddverb";
        private Recipe NeverMatchesOnAspects;
        private Recipe MatchesCoolthButNotCraftable;
        private Recipe EqualAspectsOddVerb;
        private Recipe MatchesCoolthAndWarmth;
        private Recipe MatchesCoolthEqual;
        private Recipe MatchesCoolthLess;
        private List<Recipe> Recipes;
        private RecipeCompendium rc;
        

        [SetUp]
        public void Setup()
        {

            NeverMatchesOnAspects=new Recipe() {Id="NeverMatchesOnAspects",ActionId=MATCHING_VERB,Craftable = true,Requirements = new Dictionary<string, int>() {{COOLTH,COOLTH_VALUE+10} }};
            MatchesCoolthButNotCraftable = new Recipe() { Id = "MatchesCoolthButNotCraftable", ActionId = MATCHING_VERB, Craftable = false, Requirements = new Dictionary<string, int>() { { COOLTH, COOLTH_VALUE - 1 } } };
            EqualAspectsOddVerb = new Recipe() { Id = "EqualAspectsOddVerb", ActionId = ODD_VERB, Craftable = true, Requirements = new Dictionary<string, int>() { { COOLTH, COOLTH_VALUE-1} } };
            MatchesCoolthAndWarmth = new Recipe() { Id = "MatchesCoolthAndWarmth", ActionId = MATCHING_VERB, Craftable = true, Requirements = new Dictionary<string, int>() { { COOLTH, COOLTH_VALUE }, {WARMTH,WARMTH_VALUE} } };
            MatchesCoolthEqual = new Recipe() { Id = "MatchesCoolthEqual", ActionId = MATCHING_VERB, Craftable = true, Requirements = new Dictionary<string, int>() { { COOLTH, COOLTH_VALUE } } };
            MatchesCoolthLess = new Recipe() { Id = "MatchesCoolthLess", ActionId = MATCHING_VERB, Craftable = true, Requirements = new Dictionary<string, int>() { { COOLTH, COOLTH_VALUE-1 } } };
            Recipes=new List<Recipe>() {NeverMatchesOnAspects,
                MatchesCoolthButNotCraftable,
                EqualAspectsOddVerb,
                MatchesCoolthAndWarmth,
                MatchesCoolthEqual,
                MatchesCoolthLess };
            rc=new RecipeCompendium(Recipes,new Dice(),null);

        }


        [Test]
        public void NoAspectsPresentMatchesNoRecipe()
        {
            Assert.Null(rc.GetFirstRecipeForAspectsWithVerb(new Dictionary<string, int>(), MATCHING_VERB));
        }
        [Test]
        public void VerbMatchDistinguishesRecipe()
        {
            Dictionary<string, int> aspects = new Dictionary<string, int> { { COOLTH, COOLTH_VALUE } };
            Recipe matchedOnUsualVerb = rc.GetFirstRecipeForAspectsWithVerb(aspects, MATCHING_VERB);
            Recipe matchedOnOddVerb = rc.GetFirstRecipeForAspectsWithVerb(aspects, ODD_VERB);
            Assert.AreEqual(matchedOnUsualVerb.Id, MatchesCoolthEqual.Id );
            Assert.AreEqual(matchedOnOddVerb.Id, EqualAspectsOddVerb.Id);
            
        }
        [Test]
        public void NonCraftableRecipeIsntMatched()
        {
            Dictionary<string, int> aspects = new Dictionary<string, int> { { COOLTH, COOLTH_VALUE } };
            Assert.AreNotEqual(MatchesCoolthButNotCraftable.Id,rc.GetFirstRecipeForAspectsWithVerb(aspects, MATCHING_VERB).Id);
        }


        [Test]
        public void OneAspectPresentMatchesHighestPriorityRecipe()
        {
            Dictionary<string, int> aspects = new Dictionary<string, int> {{COOLTH, COOLTH_VALUE } };
            Assert.AreEqual(MatchesCoolthEqual.Id,rc.GetFirstRecipeForAspectsWithVerb(aspects, MATCHING_VERB).Id);
        }

        [Test]
        public void TwoAspectsPresentMatchesHighestPriority()
        {
            Dictionary<string, int> aspects = new Dictionary<string, int> { { COOLTH, COOLTH_VALUE }, { WARMTH, WARMTH_VALUE } };
            Assert.AreEqual(MatchesCoolthAndWarmth.Id, rc.GetFirstRecipeForAspectsWithVerb(aspects, MATCHING_VERB).Id);
        }


    }
}