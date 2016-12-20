﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts.Interfaces;

namespace Assets.Core.Commands
{
    public class SituationCreationCommand
    {
        public IVerb Verb { get; set; }
        public Recipe Recipe { get; set; }
        public SituationState? State { get; set; }
        public float? TimeRemaining { get; set; }

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

        public ISituationStateMachine CreateSituationStateMachine(ISituationStateMachineSituationSubscriber subscriber)
        {
            var machine=new SituationStateMachine(subscriber);
            if (Recipe == null)
                return machine;


            machine.Start(Recipe);
            if (TimeRemaining == null)
                return machine;

            return new SituationStateMachine(TimeRemaining.Value,State.Value,Recipe, subscriber);
    }

}
}
