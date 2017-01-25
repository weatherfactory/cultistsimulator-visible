using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using UnityEngine;

namespace Assets.Logic
{
   public class SituationEffectExecutor
    {
        public void RunEffects(ISituationEffectCommand command, IElementStacksManager stacksManager)
        {
            var aspectsPresent = stacksManager.GetTotalAspects();
            aspectsPresent.CombineAspects(command.Recipe.Aspects);

            foreach (var stack in stacksManager.GetStacks())
            {
                var xTriggers = stack.GetXTriggers();
               foreach (var k in xTriggers.Keys)
                   if (aspectsPresent.ContainsKey(k))
                   {
                       var existingQuantity = stack.Quantity;

                      stacksManager.ModifyElementQuantity(stack.Id, -existingQuantity);
                      stacksManager.ModifyElementQuantity(xTriggers[k],existingQuantity);

                        Debug.Log("xtrigger aspect " + k + " caused " + stack.Id + " to transform into " + xTriggers[k]);
                   }
            }
            foreach (var kvp in command.GetElementChanges())
            {
                stacksManager.ModifyElementQuantity(kvp.Key, kvp.Value);
            }
        }
    }
}
