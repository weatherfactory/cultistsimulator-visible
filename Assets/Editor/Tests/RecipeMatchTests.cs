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
        private const int COOLTH_VALUE = 5;
        private const int WARMTH_VALUE = 5;
        private Recipe NeverMatches;
        private Recipe MatchesCoolthAndWarmth;
        private Recipe MatchesCoolthEqual;
        private Recipe MatchesCoolthLess;
        private List<Recipe> Recipes;
        private RecipeCompendium rc;
        

        [SetUp]
        public void Setup()
        {

            NeverMatches=new Recipe() {Id="NeverMatches",Requirements = new Dictionary<string, int>() {{COOLTH,COOLTH_VALUE+10} }};
            MatchesCoolthAndWarmth = new Recipe() { Id = "MatchesCoolthAndWarmth", Requirements = new Dictionary<string, int>() { { COOLTH, COOLTH_VALUE }, {WARMTH,WARMTH_VALUE} } };
            MatchesCoolthEqual = new Recipe() { Id = "MatchesCoolthEqual", Requirements = new Dictionary<string, int>() { { COOLTH, COOLTH_VALUE } } };
            MatchesCoolthLess = new Recipe() { Id = "MatchesCoolthLess", Requirements = new Dictionary<string, int>() { { COOLTH, COOLTH_VALUE-1 } } };
            Recipes=new List<Recipe>() {NeverMatches, MatchesCoolthAndWarmth, MatchesCoolthEqual, MatchesCoolthLess };
            rc=new RecipeCompendium(Recipes);

        }



        [Test]
        public void NoAspectsPresentMatchesNoRecipe()
        {
            Assert.Null(rc.GetFirstRecipeForAspects(new Dictionary<string, int>()));
        }

        [Test]
        public void OneAspectPresentMatchesHighestPriorityRecipe()
        {
            Dictionary<string, int> aspects = new Dictionary<string, int> {{COOLTH, COOLTH_VALUE } };
            Assert.AreEqual(MatchesCoolthEqual.Id,rc.GetFirstRecipeForAspects(aspects).Id);
        }

        [Test]
        public void TwoAspectsPresentMatchesHighestPriority()
        {
            Dictionary<string, int> aspects = new Dictionary<string, int> { { COOLTH, COOLTH_VALUE }, { WARMTH, WARMTH_VALUE } };
            Assert.AreEqual(MatchesCoolthAndWarmth.Id, rc.GetFirstRecipeForAspects(aspects).Id);
        }

    }
}