using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// This is the standard recipe behaviour: recipe is queued, time passes, recipe finishes (and may loop or fire another one)
/// See InteractiveRecipeSituation for more complex stuff
/// </summary>
   public class SealedRecipeSituation:BaseRecipeSituation
    {
        public SealedRecipeSituation(Recipe recipe, float? timeremaining, IElementsContainer inputContainer, Compendium rc) : base(recipe, timeremaining, inputContainer, rc)
        {

    }

        public override bool IsInteractive()
        {
            return false;
        }
    }

