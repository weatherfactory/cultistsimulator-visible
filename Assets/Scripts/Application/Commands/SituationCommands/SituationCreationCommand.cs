using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Commands;
using Assets.Scripts.Application.Commands.SituationCommands;
using Assets.Scripts.Application.Interfaces;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Constants;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Enums;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.States;
using UnityEngine;
using Object = UnityEngine.Object;


namespace SecretHistories.Commands
{
    public class SituationCreationCommand: ITokenPayloadCreationCommand,IEncaustment
    {
        public Verb Verb { get; set; }
        public string RecipeId { get; set; }
        public StateEnum StateForRehydration { get; set; }
        public float TimeRemaining { get; set; }
        public string OverrideTitle { get; set; } //if not null, replaces any title from the verb or recipe
        public Dictionary<string, int> Mutations { get; set; }
        public SituationPath Path { get; set; }
        public bool IsOpen { get; set; }
        public List<Token> TokensToMigrate=new List<Token>();

        public SituationCommandQueue CommandQueue { get; set; }=new SituationCommandQueue();
        private WindowCreationCommand windowCreationCommand;

        public SituationCreationCommand()
        {

        }

        public SituationCreationCommand(Verb verb, string recipeId, StateEnum state)
        {

            RecipeId = recipeId;
            Verb = verb;
            StateForRehydration = state;
            Path =new SituationPath(verb);
            CommandQueue = new SituationCommandQueue();
        }


        
        public ITokenPayload Execute(Context context)
        {
            SituationsCatalogue situationsCatalogue = Watchman.Get<SituationsCatalogue>();
            var registeredSituations = situationsCatalogue.GetRegisteredSituations();


            var recipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(RecipeId);

            if (registeredSituations.Exists(rs => rs.Unique && rs.Verb.Id == Verb.Id))
            {
                NoonUtility.Log("Tried to create " + recipe.Id + " for verb " + recipe.ActionId + " but that verb is already active.");
                    return NullSituation.Create();
            }

            Situation newSituation = new Situation(Path);

            newSituation.State = SituationState.Rehydrate(StateForRehydration, newSituation);
            newSituation.Verb = Verb;
            newSituation.ActivateRecipe(recipe);
            newSituation.ReduceLifetimeBy(recipe.Warmup - TimeRemaining);
            newSituation.OverrideTitle = OverrideTitle;


            if (TokensToMigrate.Any())
                newSituation.AcceptTokens(SphereCategory.SituationStorage,TokensToMigrate);
            


            var windowSpherePath=new SpherePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWindowSpherePath); 
            var windowLocation =
                new TokenLocation(Vector3.zero,windowSpherePath); //it shouldn't really be zero, but we don't know the real token loc in the current flow


            windowCreationCommand = new WindowCreationCommand(windowLocation);

            if (windowCreationCommand!=null)
            { 
                var newWindow = windowCreationCommand.Execute(Context.Unknown());
                newWindow.Attach(newSituation,windowLocation); }

            newSituation.CommandQueue.AddCommandsFrom(CommandQueue);



            newSituation.ExecuteHeartbeat(0f);

            return newSituation;


        }

    }
}
