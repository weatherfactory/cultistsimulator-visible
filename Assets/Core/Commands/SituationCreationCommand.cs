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

		public ISituationAnchor SourceAnchor { get; set; } // this may not be set if no origin is known or needed
        public IVerb Verb { get; set; }
        public Species Species
        {
            get { return Verb.Species; }
        }
        public Recipe Recipe { get; set; }
        public SituationState State { get; set; }
        public float? TimeRemaining { get; set; }
        public string OverrideTitle { get; set; } //if not null, replaces any title from the verb or recipe
        public TokenLocation AnchorLocation { get; set; }
        public TokenLocation WindowLocation { get; set; }
        public List<SlotSpecification> OngoingSlots { get; set; } //we might, eg, save when slots have been created by a recipe, but later move on to another recipe
        public SpherePath SituationPath { get; set; }
        public bool Open { get; set; }

        public SituationCreationCommand(IVerb verb, Recipe recipe, SituationState situationState,
            TokenLocation anchorLocation, ISituationAnchor sourceAnchor = null)
        {
            if (recipe == null && verb == null)
                throw new ArgumentException("Must specify either a recipe or a verb (or both");


            Recipe = recipe;
            Verb = verb;
            AnchorLocation = anchorLocation;
            SourceAnchor = sourceAnchor;
            State = situationState;
            SituationPath = SpherePath.SituationPath(verb);
            OngoingSlots = new List<SlotSpecification>();
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
