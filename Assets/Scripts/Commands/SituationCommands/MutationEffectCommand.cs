using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Commands.SituationCommands
{
   public class MutationEffectCommand
    {
    }


   public class XTriggerCommand
   {

   }

   public class DeckEffectCommand
   {

   }

   public class RecipeEffectCommand
   {

   }

   public class VerbManipulationCommand
   {}

   public class ElementPurgeCommand
   {}

   public class ConsumptionCommand
   {}
   //note: xtriggers for recipe aspects happen before xtriggers for card aspects. Within that precedence, aspects take effect in non-specific order.
   //I think this will generally make sense. Recipe aspects are 'specifically, I want to do this'
   //Card aspects are 'this should generally happen'
   //If this basic logic doesn't work, solutions under consideration: (1) xtrigger priorities (2) feeding a stack back in if it's transformed to react to its new xtriggers (with guard against loop)

   //RunXTriggers(sphere, recipeAspects, dice);
   //RunXTriggers(sphere, cardAspects, dice);

   ////note: standard effects happen *after* XTrigger effects
   //RunDeckEffect(command, sphere, storage);
   ////and after deck effect
   //RunRecipeEffects(command, sphere);

   ////Penultimate: run verb manipulations and element purges. This means purges will occur *after* any elements have been mutated or xtrigger-transformed.

   //RunVerbManipulations(command, _ttm);
   ////Element purges are run after verb manipulations. This is so we can halt a verb and then delete any applicable contents (rather than deleting the verb, which is possible but very risky if it contains plot-relevant elements!)
   //RunElementPurges(command, _ttm);

   ////Do this last: remove any stacks marked for consumption by being placed in a consuming slot
   //RunConsumptions(
   //sphere); //NOTE: If a stack has just been transformed into another element, all sins are forgiven. It won't be consumed.
}
