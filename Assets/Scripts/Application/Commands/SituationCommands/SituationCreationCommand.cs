using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
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
        public StateEnum StateIdentifier { get; set; }
        public float TimeRemaining { get; set; }
        public Dictionary<string, int> Mutations { get; set; }
        //This is used for reference and repair - perhaps testing. I'm populating the parent path in the Execute paramater
        public bool IsOpen { get; set; }
        public List<PopulateDominionCommand> Dominions { get; set; }=new List<PopulateDominionCommand>();


        public Situation LastSituationCreated;

        public List<ISituationCommand> CommandQueue { get; set; } = new List<ISituationCommand>();

        public SituationCreationCommand()
        {
            StateIdentifier = StateEnum.Inchoate;
        }

        [JsonConstructor]
        public SituationCreationCommand(string verbId): this()
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
            StateIdentifier = state;
            return this;
        }

        public SituationCreationCommand WithRecipeAboutToActivate(string recipeId)
        {
            StateIdentifier = StateEnum.Unstarted;
            CommandQueue.Add(TryActivateRecipeCommand.OverridingRecipeActivation(recipeId));
            return this;
        }



        public ITokenPayload Execute(Context context)
        {
            HornedAxe situationsCatalogue = Watchman.Get<HornedAxe>();
            var registeredSituations = situationsCatalogue.GetRegisteredSituations();


            var recipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(RecipeId);

            var verb = Watchman.Get<Compendium>().GetEntityById<Verb>(VerbId);

            if (registeredSituations.Exists(rs => rs.Unique && rs.Verb.Id == VerbId))
            {
                NoonUtility.Log("Tried to create " + recipe.Id + " for verb " + verb.Id + " but that verb is already active.");
                    return NullSituation.Create();
            }

            //If we deserialise a situation, we'll already know its ID. If we're creating it for the first time, we need to pick an ID
            if (String.IsNullOrEmpty(Id)) 
                 Id = verb.DefaultUniqueTokenId();

            var newSituation = BuildSituation(verb, recipe);

            //This must go here, as soon as the situation is created and before tokens or commands are added, because it's here that the situation spheres get attached.
            AttachWindow(newSituation);

            //and we can't display text before the window is attached; until then, there's no notes dominion attached to put the notes in
            DisplayText(verb, recipe, newSituation);


            foreach (var d in Dominions)
                d.Execute(newSituation);

            //this may have been specified if the new situation is being spawned from an old one
        
            newSituation.AddCommandsFrom(CommandQueue);


            LastSituationCreated = newSituation; //in case we need to retrieve a just-created situation later than this method call - as we do for instance in SpawnNewTokenCommand

            return newSituation;


        }

        private Situation BuildSituation(Verb verb, Recipe recipe)
        {
            Situation newSituation = new Situation(verb, Id);


            newSituation.State = SituationState.Rehydrate(StateIdentifier, newSituation);

            newSituation.SetRecipeActive(recipe);
            newSituation.ReduceLifetimeBy(recipe.Warmup - TimeRemaining);
            return newSituation;
        }

        private static void DisplayText(Verb verb, Recipe recipe, Situation newSituation)
        {
            if (verb.IsValid()) //we might create a situation with a null verb, for example if we're rehydrating. If it's a real verb, then base a starting recipe prediction on it.
            {
                var initialPredictionFromVerb = new RecipePrediction(recipe, AspectsDictionary.Empty(), verb);
                newSituation.ReactToLatestRecipePrediction(initialPredictionFromVerb,
                    new Context(Context.ActionSource.SituationCreated));
            }
        }

        private void AttachWindow(Situation newSituation)
        {
            var
                windowSphere =
                    FucineRoot.Get()
                        .GetWindowsSphere(); //we can't check the windowsphere from the sphere the situation token's going in, because we don't know what that will be yet... so we might need to move the window later
            var windowLocation =
                new TokenLocation(Vector3.zero,
                    windowSphere
                        .GetAbsolutePath()); //it shouldn't really be zero, but we don't know the real token loc in the current flow

            var sphereToDisplayWindowIn = Watchman.Get<HornedAxe>().GetSphereByPath(windowLocation.AtSpherePath);
            var windowType = newSituation.GetWindowType();
            var newWindow = Watchman.Get<PrefabFactory>()
                .CreateWindowPrefab(windowType, sphereToDisplayWindowIn.transform);
            //var newWindow = Watchman.Get<PrefabFactory>().CreateLocally<SituationWindow>(sphereToDisplayWindowIn.transform);
            newWindow.Attach(newSituation);


            if (IsOpen)
                newSituation.OpenAt(windowLocation);
        }
    }
}
