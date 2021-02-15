using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Commands;
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
        public string VerbId { get; set; }
        public string RecipeId { get; set; }
        public StateEnum StateForRehydration { get; set; }
        public float TimeRemaining { get; set; }
        public string OverrideTitle { get; set; } //if not null, replaces any title from the verb or recipe
        public Dictionary<string, int> Mutations { get; set; }
        public SituationPath Path { get; set; }
        public bool IsOpen { get; set; }
        public List<Token> TokensToMigrate=new List<Token>();

        public SituationCommandQueue CommandQueue { get; set; }=new SituationCommandQueue();

        public SituationCreationCommand()
        {

        }

        public SituationCreationCommand(string verbId, string recipeId, SituationPath path, StateEnum state)
        {

            RecipeId = recipeId;
            VerbId = verbId;
            Path = path;
            StateForRehydration = state;
            CommandQueue = new SituationCommandQueue();
        }


        
        public ITokenPayload Execute(Context context)
        {
            SituationsCatalogue situationsCatalogue = Watchman.Get<SituationsCatalogue>();
            var registeredSituations = situationsCatalogue.GetRegisteredSituations();


            var recipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(RecipeId);

            var verb = Watchman.Get<Compendium>().GetEntityById<Verb>(VerbId);

            if (registeredSituations.Exists(rs => rs.Unique && rs.Verb.Id == VerbId))
            {
                NoonUtility.Log("Tried to create " + recipe.Id + " for verb " + recipe.ActionId + " but that verb is already active.");
                    return NullSituation.Create();
            }

            if (!Path.IsValid())
                throw new ApplicationException($"trying to create a situation with an invalid path: '{Path}'");


            Situation newSituation = new Situation(Path, verb);
            newSituation.State = SituationState.Rehydrate(StateForRehydration, newSituation);

            newSituation.ActivateRecipe(recipe);
            newSituation.ReduceLifetimeBy(recipe.Warmup - TimeRemaining);
            newSituation.OverrideTitle = OverrideTitle;
            

            //This MUSt go here, as soon as the situation is created and before tokens or commands are added, because it's here that the situation spheres get attached.
            //which I don't love: this whole setup is still hinky
            var windowSpherePath = new SpherePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWindowSpherePath);
            var windowLocation =
                new TokenLocation(Vector3.zero, windowSpherePath); //it shouldn't really be zero, but we don't know the real token loc in the current flow

            var sphere = Watchman.Get<SphereCatalogue>().GetSphereByPath(windowLocation.AtSpherePath);
            var newWindow = Watchman.Get<PrefabFactory>().CreateLocally<SituationWindow>(sphere.transform);
            newWindow.Attach(newSituation);

            
            if (TokensToMigrate.Any())
                newSituation.AcceptTokens(SphereCategory.SituationStorage,TokensToMigrate);
            

            newSituation.CommandQueue.AddCommandsFrom(CommandQueue);
            newSituation.ExecuteHeartbeat(0f);

            newSituation.NotifyStateChange();
            newSituation.NotifyTimerChange();

            return newSituation;


        }

    }
}
