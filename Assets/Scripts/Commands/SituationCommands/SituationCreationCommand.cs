using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;

namespace Assets.Core.Commands
{
    public class SituationCreationCommand
    {

		public Token SourceToken { get; set; } // this may not be set if no origin is known or needed
        public IVerb Verb { get; set; }
        
        public Recipe Recipe { get; set; }
        public StateEnum State { get; set; }
        public float? TimeRemaining { get; set; }
        public string OverrideTitle { get; set; } //if not null, replaces any title from the verb or recipe
        public TokenLocation AnchorLocation { get; set; }
        public TokenLocation WindowLocation { get; set; }

        public SituationPath SituationPath { get; set; }
        public bool Open { get; set; }

        public List<ISituationCommand> Commands=new List<ISituationCommand>();

        public SituationCreationCommand(IVerb verb, Recipe recipe, StateEnum state,
            TokenLocation anchorLocation, Token sourceToken = null)
        {
            if (recipe == null && verb == null)
                throw new ArgumentException("Must specify either a recipe or a verb (or both");

            Recipe = recipe;
            Verb = verb;
            AnchorLocation = anchorLocation;
            SourceToken = sourceToken;
            State = state;
            SituationPath =new SituationPath(verb);
        }

        public IVerb GetBasicOrCreatedVerb()
        {
            return Registry.Get<Compendium>().GetVerbForRecipe(Recipe);
        }

    }
}
