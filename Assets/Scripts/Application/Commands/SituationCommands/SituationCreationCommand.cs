using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Commands;
using Assets.Scripts.Application.Fucine;
using Assets.Scripts.Application.Interfaces;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Constants;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Enums;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.States;
using UnityEngine;
using Object = UnityEngine.Object;


namespace SecretHistories.Commands
{
    public class SituationCreationCommand: ITokenPayloadCreationCommand,IEncaustment
    {
        public string Id { get; set; }
        public string VerbId { get; set; }
        public string RecipeId { get; set; }
        public int Quantity { get; set; }
        public StateEnum StateForRehydration { get; set; }
        public float TimeRemaining { get; set; }
        public string OverrideTitle { get; set; } //if not null, replaces any title from the verb or recipe
        public Dictionary<string, int> Mutations { get; set; }
        //This is used for reference and repair - perhaps testing. I'm populating the parent path in the Execute paramater
        public bool IsOpen { get; set; }
        public List<Token> TokensToMigrate=new List<Token>();
        public List<PopulateDominionCommand> Dominions { get; set; }=new List<PopulateDominionCommand>();

        public SituationCommandQueue CommandQueue { get; set; }=new SituationCommandQueue();

        public SituationCreationCommand()
        {
            StateForRehydration = StateEnum.Unknown;
            CommandQueue = new SituationCommandQueue();
        }

        [JsonConstructor]
        public SituationCreationCommand(string verbId, FucinePath cachedParentPath): this()
        {
            VerbId = verbId;
        }

        public SituationCreationCommand WithRecipeId(string withRecipeId)   
        {
            RecipeId = withRecipeId;
            return this;
        }

        public SituationCreationCommand WithVerbId(string verbId)
        {
            VerbId = verbId;
            return this;
        }


        public SituationCreationCommand AlreadyInState(StateEnum state)
        {
            StateForRehydration = state;
            return this;
        }

        public ITokenPayload Execute(Context context, Sphere sphere)
        {
            HornedAxe situationsCatalogue = Watchman.Get<HornedAxe>();
            var registeredSituations = situationsCatalogue.GetRegisteredSituations();


            var recipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(RecipeId);

            var verb = Watchman.Get<Compendium>().GetEntityById<Verb>(VerbId);

            if (registeredSituations.Exists(rs => rs.Unique && rs.Verb.Id == VerbId))
            {
                NoonUtility.Log("Tried to create " + recipe.Id + " for verb " + recipe.ActionId + " but that verb is already active.");
                    return NullSituation.Create();
            }

            //If we deserialise a situation, we'll already know its ID. If we're creating it for the first time, we need to pick an ID
            if (String.IsNullOrEmpty(Id)) 
                Id = Id = verb.DefaultUniqueTokenId();

            Situation newSituation = new Situation(verb,Id);

           
            newSituation.State = SituationState.Rehydrate(StateForRehydration, newSituation);

            newSituation.ActivateRecipe(recipe);
            newSituation.ReduceLifetimeBy(recipe.Warmup - TimeRemaining);
            newSituation.OverrideTitle = OverrideTitle;
            

            //This MUST go here, as soon as the situation is created and before tokens or commands are added, because it's here that the situation spheres get attached.
            var windowSphere = newSituation.GetWindowsSphere();
            var windowLocation =
                new TokenLocation(Vector3.zero, windowSphere.GetAbsolutePath()); //it shouldn't really be zero, but we don't know the real token loc in the current flow

            var sphereToDisplayWindowIn = Watchman.Get<HornedAxe>().GetSphereByPath(windowLocation.AtSpherePath);
            var newWindow = Watchman.Get<PrefabFactory>().CreateLocally<SituationWindow>(sphereToDisplayWindowIn.transform);
            newWindow.Attach(newSituation);


            foreach (var d in Dominions) //there's a risk here. If we automatically clear dismissed dominions, then we might conceivably dismiss one we've just populated here
                d.Execute(newSituation);

            //this may have been specified if the new situation is being spawned from an old one
            if (TokensToMigrate.Any())
                newSituation.AcceptTokens(SphereCategory.SituationStorage,TokensToMigrate);
            

            newSituation.CommandQueue.AddCommandsFrom(CommandQueue);
            
            return newSituation;


        }


    }
}
