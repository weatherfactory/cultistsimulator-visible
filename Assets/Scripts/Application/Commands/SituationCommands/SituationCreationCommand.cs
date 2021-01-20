using System;
using System.Collections.Generic;
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
            var anchorSphere = sphereCatalogue.GetSphereByPath(AnchorLocation.AtSpherePath);
            var windowSphere = sphereCatalogue.GetSphereByPath(new SpherePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWindowSpherePath));

            var newAnchor = AttachNewAnchor(AnchorLocation.Anchored3DPosition, newSituation, anchorSphere);
            var newWindow = AttachNewWindow(windowSphere, newAnchor, newSituation);



            if (Open)
                newSituation.OpenAtCurrentLocation();
            else
                newSituation.Close();

            foreach (var c in Commands)
                newSituation.CommandQueue.AddCommand(c);

            SoundManager.PlaySfx("SituationTokenCreate");

            if(SourceToken!=null)
            {
                var spawnedTravelItinerary = new TokenTravelItinerary(SourceToken.TokenRectTransform.anchoredPosition3D,
                        anchorSphere.Choreographer.GetFreeLocalPosition(newAnchor, SourceToken.ManifestationRectTransform.anchoredPosition))
                    .WithDuration(1f)
                    .WithSphereRoute(windowSphere, anchorSphere)
                    .WithScaling(0f, 1f);

                newAnchor.TravelTo(spawnedTravelItinerary, new Context(Context.ActionSource.SpawningAnchor));
             }

            return newSituation;


        }

        private Token AttachNewAnchor(Vector3 position, Situation situation, Sphere anchorSphere)
        {
            var newAnchor = Watchman.Get<PrefabFactory>().CreateLocally<Token>(anchorSphere.transform);
            situation.AttachAnchor(newAnchor);
            anchorSphere.AcceptToken(newAnchor, new Context(Context.ActionSource.Unknown));
            newAnchor.transform.localPosition = position;
            return newAnchor;
        }

        private SituationWindow AttachNewWindow(Sphere windowSphere, Token newAnchor, Situation situation)
        {
            var newWindow = Watchman.Get<PrefabFactory>().CreateLocally<SituationWindow>(windowSphere.transform);

            newWindow.transform.SetParent(windowSphere.transform);
            newWindow.positioner.Initialise(newAnchor);
            situation.AttachWindow(newWindow);
            return newWindow;
        }




        private IVerb GetBasicOrCreatedVerb()
        {
            return Watchman.Get<Compendium>().GetVerbForRecipe(Recipe);
        }

    }
}
