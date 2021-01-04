using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.Services;

namespace SecretHistories.Commands
{
   public class RecipePrediction
   {
       private Recipe _actualRecipe;
        public string Title { get; protected set; }
        public string DescriptiveText { get; protected set; }
        public string BurnImage => _actualRecipe.BurnImage;
        public EndingFlavour SignalEndingFlavour => _actualRecipe.SignalEndingFlavour;
        public bool Craftable => _actualRecipe.Craftable;
        public bool HintOnly => _actualRecipe.HintOnly;

        public static RecipePrediction DefaultFromVerb(IVerb verb)
        {
            NullRecipe nullRecipe=NullRecipe.Create(verb);
            return new RecipePrediction(nullRecipe,new AspectsDictionary());
        }


        public RecipePrediction(Recipe actualRecipe, IAspectsDictionary aspectsAvailable)
        {
            _actualRecipe = actualRecipe;
            Title = actualRecipe.Label;
            DescriptiveText = actualRecipe.StartDescription;
            TextRefiner tr = new TextRefiner(aspectsAvailable);
            DescriptiveText = tr.RefineString(DescriptiveText);

        }
    }
}
