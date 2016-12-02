using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

namespace Assets.Core.Commands
{
    public class SituationCreationCommand
    {
        public IVerb Verb { get; set; }
        public Recipe Recipe { get; set; }
        public SituationState? State { get; set; }
        public int? TimeRemaining { get; set; }

        public SituationCreationCommand(IVerb verb,Recipe recipe)
        {
            if(recipe==null && verb==null)
            { throw new ArgumentException("Must specify either a recipe or a verb (or both");}
            Recipe = recipe;
            Verb = verb;

        }

        public IVerb GetBasicOrCreatedVerb()
        {
            if (Verb == null)
            {
                return new CreatedVerb(Recipe.ActionId, Recipe.Label, Recipe.Description);
            }

            return Verb;
        }
}
}
