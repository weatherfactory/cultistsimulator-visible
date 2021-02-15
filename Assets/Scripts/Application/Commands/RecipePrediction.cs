using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.Services;

namespace SecretHistories.Commands
{
   public class RecipePrediction: IEquatable<RecipePrediction>
   {
       private readonly Recipe _actualRecipe;
       public string RecipeId => _actualRecipe.Id;
        public string Title { get; protected set; }
        public string DescriptiveText { get; protected set; }
        public string BurnImage => _actualRecipe.BurnImage;
        public EndingFlavour SignalEndingFlavour => _actualRecipe.SignalEndingFlavour;
        public bool Craftable => _actualRecipe.Craftable;
        public bool HintOnly => _actualRecipe.HintOnly;

        public static RecipePrediction DefaultFromVerb(Verb verb)
        {
            Recipe hintRecipe= Recipe.CreateSpontaneousHintRecipe(verb);
            return new RecipePrediction(hintRecipe, new AspectsDictionary());
        }


        public RecipePrediction(Recipe actualRecipe, AspectsDictionary aspectsAvailable)
        {
            _actualRecipe = actualRecipe;
            Title = actualRecipe.Label;
            DescriptiveText = actualRecipe.StartDescription;
            TextRefiner tr = new TextRefiner(aspectsAvailable);
            DescriptiveText = tr.RefineString(DescriptiveText);

        }

        public bool Equals(RecipePrediction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_actualRecipe, other._actualRecipe) && Title == other.Title && DescriptiveText == other.DescriptiveText;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RecipePrediction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_actualRecipe != null ? _actualRecipe.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Title != null ? Title.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DescriptiveText != null ? DescriptiveText.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(RecipePrediction left, RecipePrediction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RecipePrediction left, RecipePrediction right)
        {
            return !Equals(left, right);
        }

        public bool AddsMeaningfulInformation(RecipePrediction currentRecipePrediction)
        {

            if (currentRecipePrediction == null) //if there's no existing prediction, this has to be an improvement
                return true;

            if (currentRecipePrediction == this) //if this and we are the same, forget about it
                return false;

            //we often add a . to indicate that the description is intentionally empty.
            //if we do that, or if it's a mistaken empty string, just go back.


            if (!Situation.TextIntendedForDisplay(DescriptiveText))
                return false;

            return true;
        }
   }
}
