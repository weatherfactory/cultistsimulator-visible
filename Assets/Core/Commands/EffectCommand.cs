using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.Core
{
    public class EffectCommand: IEffectCommand
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public Recipe Recipe { get; set; }


        public EffectCommand(Dictionary<string, int> elementChanges)
        {
            Title = "default title";
            Description = "default message";
        }

        public EffectCommand(Recipe recipe)
        {
            Recipe = recipe;
            Title = "default title";
            Description = recipe.Description;
        }

        public Dictionary<string, int> GetElementChanges()
        {
            return Recipe.Effects;
        }

    }
}
