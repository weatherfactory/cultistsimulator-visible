using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.Core
{
    public class SituationEffectCommand: ISituationEffectCommand
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Recipe Recipe { get; set; }
        public bool AsNewSituation { get; set; } //determines whether the recipe will spawn a new situation.
        //this currently happens if the verb is different from the original verb *and* if it's specified as additional

        public SituationEffectCommand(Recipe recipe,bool asNewSituation)
        {
            Recipe = recipe;
            Title = "default title";
            Description = recipe.Description;
            AsNewSituation = asNewSituation;
        }

        public Dictionary<string, int> GetElementChanges()
        {
            return Recipe.Effects;
        }

        /// <summary>
        /// returns the deck to draw from if there is one, or null if there isn't one
        /// </summary>
        /// <returns></returns>
        public new Dictionary<string, int> GetDeckEffects()
        {
            //we only return the name of the deck. Implementation of drawing is up to classes with access to a compendium.

            return Recipe.DeckEffects;
        }


    }
}
