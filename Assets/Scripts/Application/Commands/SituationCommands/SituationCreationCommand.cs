using System;
using System.Collections.Generic;
using Assets.Scripts.Application.Commands;
using Assets.Scripts.Application.Commands.SituationCommands;
using Assets.Scripts.Application.Interfaces;
using Newtonsoft.Json;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Enums;
using SecretHistories.Services;
using SecretHistories.States;
using UnityEngine;
using Object = UnityEngine.Object;


namespace SecretHistories.Commands
{
    public class SituationCreationCommand: ISaveable
    {

		public Token SourceToken { get; set; } // this may not be set if no origin is known or needed
        public IVerb Verb { get; set; }
        
        public Recipe Recipe { get; set; }
        public StateEnum State { get; set; }
        public float? TimeRemaining { get; set; }
        public string OverrideTitle { get; set; } //if not null, replaces any title from the verb or recipe
        public TokenLocation AnchorLocation { get; set; }

        public SituationPath SituationPath { get; set; }
        public bool Open { get; set; }

        public List<ISituationCommand> Commands=new List<ISituationCommand>();
        private TokenCreationCommand tokenCreationCommand;
        private WindowCreationCommand windowCreationCommand;

        public SituationCreationCommand(IVerb verb, Recipe recipe, StateEnum state,
            TokenLocation anchorLocation)
        {
            if (recipe == null && verb == null)
                throw new ArgumentException("Must specify either a recipe or a verb (or both");

            Recipe = recipe;
            Verb = verb;
            AnchorLocation = anchorLocation;
            State = state;
            SituationPath =new SituationPath(verb);
        }

        public SituationCreationCommand WithDefaultAttachments()
        {
            tokenCreationCommand = new TokenCreationCommand(Verb, AnchorLocation, SourceToken);
            windowCreationCommand=new WindowCreationCommand(new SpherePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWindowSpherePath));

            return this;
        }

  

        public Situation Execute(SituationsCatalogue situationsCatalogue)
        {
            Situation newSituation = new Situation(SituationPath);
            situationsCatalogue.RegisterSituation(newSituation);
            newSituation.Verb = GetBasicOrCreatedVerb();
            newSituation.TimeRemaining = TimeRemaining ?? 0;
            newSituation.CurrentPrimaryRecipe = Recipe;
            newSituation. OverrideTitle = OverrideTitle;
            newSituation.CurrentState = SituationState.Rehydrate(State, newSituation);


            var sphereCatalogue = Watchman.Get<SphereCatalogue>();

            if(tokenCreationCommand!=null)
            {
                var newAnchor=tokenCreationCommand.Execute(sphereCatalogue);
                newSituation.Attach(newAnchor);
            }
            
            if(windowCreationCommand!=null)
            { 
                var newWindow = windowCreationCommand.Execute(sphereCatalogue);
               newSituation.Attach(newWindow);
            }


            if (Open)
                newSituation.OpenAtCurrentLocation();
            else
                newSituation.Close();

            foreach (var c in Commands)
                newSituation.CommandQueue.AddCommand(c);

            return newSituation;


        }


        private IVerb GetBasicOrCreatedVerb()
        {
            return Watchman.Get<Compendium>().GetVerbForRecipe(Recipe);
        }

        public SituationCreationCommand(Situation basedOnSituation)
        {
            Verb = basedOnSituation.Verb;
            Recipe = basedOnSituation.CurrentPrimaryRecipe;
            //State = basedOnSituation.CurrentState;
            TimeRemaining = basedOnSituation.TimeRemaining;
       //OverrideTitle { get; set; } //if not null, replaces any title from the verb or recipe
       AnchorLocation = basedOnSituation.GetAnchorLocation();
       SituationPath = basedOnSituation.Path;
       Open = basedOnSituation.IsOpen;
       Commands = basedOnSituation.CommandQueue.GetCurrentCommandsAsList();
        }


        public string ToJson()
        {
            string output = JsonConvert.SerializeObject(this);
            return output;
        }
    }
}
