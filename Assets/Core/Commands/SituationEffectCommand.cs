using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
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
        public Expulsion Expulsion { get; set; }

        public SituationEffectCommand(Recipe recipe,bool asNewSituation,Expulsion expulsion)
        {
            Recipe = recipe;
            Title = "default title";
            Description = recipe.Description;
            AsNewSituation = asNewSituation;
            Expulsion = expulsion;
        }

        public Dictionary<string, string> GetElementChanges()
        {
            return Recipe.Effects;
        }

        /// <summary>
        /// returns the deck to draw from if there is one, or null if there isn't one
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetDeckEffects()
        {
            //we only return the name of the deck. Implementation of drawing is up to classes with access to a compendium.

            return Recipe.DeckEffects;
        }


    }
}
