using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CS.Tests
{
   public  class RecipeExecutionTests
    {
        [Test]
        public void AlternativeRecipeDoesntExecute_IfNoRequirements_AndDiceRollUnsatisfied()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void AlternateRecipeExecutes_IfNoRequirements_AndDiceRollSatisfied()
        {
            throw new NotImplementedException();

        }


        [Test]
        public void AlternateRecipeExecutes_IfRequirementsSatisfied()
        {
            throw new NotImplementedException();

        }

        [Test]
        public void AlternateRecipeDoesntExecute_IfDiceRollUnsatisfied()
        {
            throw new NotImplementedException();

        }



        // alternativerecipes: //these will be completed in place of this recipe if (1) their requirements are satisfied *by concrete possessed resources, not considering those resources' aspects;
        //and (2) if we roll <=chance on d100"
        //if additional=true, they'll execute as well as, not instead of, the original recipe
        //loop: recipeid //this, or another recipe, may begin when this completes. NB if an alternative recipe is triggered, the loop from that will apply instead.
    }
}
