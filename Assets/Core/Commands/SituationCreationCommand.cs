using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi;
using Assets.CS.TabletopUI;

namespace Assets.Core.Commands
{
    public class SituationCreationCommand
    {

		public DraggableToken SourceToken { get; set; } // this may not be set if no origin is known or needed
        public IVerb Verb { get; set; }
        public Recipe Recipe { get; set; }
        public SituationState State { get; set; }
        public float? TimeRemaining { get; set; }

		public SituationCreationCommand(IVerb verb,Recipe recipe, SituationState situationState, DraggableToken sourceToken = null)
		{
			if (recipe==null && verb==null)
				throw new ArgumentException("Must specify either a recipe or a verb (or both");
			
			Recipe = recipe;
			Verb = verb;
			SourceToken = sourceToken;
		    State = situationState;
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
