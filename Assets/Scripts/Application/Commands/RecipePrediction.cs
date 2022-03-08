using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Services;

namespace SecretHistories.Commands
{
   public class RecipePrediction: IEquatable<RecipePrediction>,INotification
   {
       private readonly Recipe _predictingRecipe;
       public string RecipeId => _predictingRecipe.Id;
        public string Title { get; protected set; }
        public string Description { get; protected set; }
        public bool Additive { get; protected set; }

        public int EmphasisLevel
        {
            get
            {
                if (HintOnly)
                    return -1;
                return 0;
            }
        }
        public EndingFlavour SignalEndingFlavour => _predictingRecipe.SignalEndingFlavour;

        //odd snarl of states here.
        public bool Craftable => _predictingRecipe.Craftable;
            //Nothing is ever Craftable and HintOnly, but some things are Craftable/HintOnly and some are Craftable/Not HintOnly
        public bool HintOnly => _predictingRecipe.HintOnly;
        public bool IsBaseVerbRecipe()
        {
            if (!string.IsNullOrEmpty(_predictingRecipe.ActionId) && string.IsNullOrEmpty(_predictingRecipe.Id))
                return true;

            return false;
        }

        //public static RecipePrediction DefaultFromVerb(Verb verb)
        //{
        //    Recipe hintRecipe= Recipe.CreateSpontaneousHintRecipe(verb);
        //    return new RecipePrediction(hintRecipe, new AspectsDictionary());
        //}


        public RecipePrediction(Recipe predictingRecipe, AspectsDictionary aspectsAvailable,Verb withVerb)
        {
            if (!predictingRecipe.IsValid())
            { //it's a null recipe, or for some other reason not useful, fall back to the basic verb info.
                predictingRecipe = Recipe.CreateSpontaneousHintRecipe(withVerb);
                Additive = false;// new day, base verb description, we want to clear out all other notes.
            }
            else
                Additive = true;    

            _predictingRecipe = predictingRecipe;

            TextRefiner tr = new TextRefiner(aspectsAvailable);
            Title = tr.RefineString(predictingRecipe.Label);
            Description = tr.RefineString(predictingRecipe.StartDescription);
            Description = predictingRecipe.StartDescription;

            Description = tr.RefineString(Description);
        }

        public bool Equals(RecipePrediction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_predictingRecipe, other._predictingRecipe) && Title == other.Title && Description == other.Description;
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
                var hashCode = (_predictingRecipe != null ? _predictingRecipe.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Title != null ? Title.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
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

            if (currentRecipePrediction.Craftable != Craftable)
                return true;
            //We've moved from a craftable to a non-craftable, even if the text is the same. Which we always care about.

            if (currentRecipePrediction.IsBaseVerbRecipe()) //Without a recipe prediction update, start button doesn't become available, so this allows us to have craftable honoured when there's no meaningful info in the startdescription
                return true;

            if (currentRecipePrediction == this) //if this and we are the same, forget about it
                return false;

            //we often add a . to indicate that the description is intentionally empty.
            //if we do that, or if it's a mistaken empty string, just go back.

            

            if (!Situation.TextIntendedForDisplay(Description))
                return false;

            return true;
        }
   }
}
