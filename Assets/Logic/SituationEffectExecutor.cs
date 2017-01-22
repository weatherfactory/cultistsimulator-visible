using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;

namespace Assets.Logic
{
   public class SituationEffectExecutor
    {
        public static void RunEffects(ISituationEffectCommand command, IElementStacksManager stacksManager)
        {
            foreach (var kvp in command.GetElementChanges())
            {
                stacksManager.ModifyElementQuantity(kvp.Key, kvp.Value);
            }
        }
    }
}
