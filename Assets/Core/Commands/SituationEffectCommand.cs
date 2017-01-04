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
        public bool AsNewSituation { get; set; } //determines whether the recipe will 

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


    }
}
