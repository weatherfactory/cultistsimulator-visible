using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.CS.TabletopUI
{
    public class SituationEventData
    {
        public float Warmup { get; set; }
        public float TimeRemaining { get; set; }
        public IVerb ActiveVerb { get; set; }
        public Recipe CurrentRecipe; //replace with SituationCreationComand / SituationEffectCommand? combine CreationCommand and EffectCommand?
        public RecipePrediction RecipePrediction;

        public Dictionary<SphereCategory, List<ElementStackToken>> StacksInEachStorage { get; set; }
        public INotification Notification;
        public RecipeBeginningEffectCommand BeginningEffectCommand;
        public RecipeCompletionEffectCommand CompletionEffectCommand;
        public SituationState SituationState;
        public bool IsOpen;
        public float IntervalForLastHeartbeat { get; set; }

        public static SituationEventData Create(Situation fromSituation)
        {
            var e = new SituationEventData();
            e.Warmup = fromSituation.Warmup;
            e.TimeRemaining = fromSituation.TimeRemaining;
            e.CurrentRecipe = fromSituation.currentPrimaryRecipe;
            e.RecipePrediction = fromSituation.CurrentRecipePrediction;
            e.StacksInEachStorage.Add(SphereCategory.Threshold,fromSituation.GetStacks(SphereCategory.Threshold));
            e.StacksInEachStorage.Add(SphereCategory.SituationStorage, fromSituation.GetStacks(SphereCategory.SituationStorage));
            e.StacksInEachStorage.Add(SphereCategory.Output, fromSituation.GetStacks(SphereCategory.Output));
            e.ActiveVerb = fromSituation.Verb;
            e.SituationState = fromSituation.State;
            e.BeginningEffectCommand = fromSituation.CurrentBeginningEffectCommand;
            e.CompletionEffectCommand = fromSituation.currentCompletionEffectCommand;
            e.IntervalForLastHeartbeat = fromSituation.intervalForLastHeartbeat;
            e.IsOpen = fromSituation.IsOpen;
            return e;

        }



        private SituationEventData()
        {
            StacksInEachStorage=new Dictionary<SphereCategory, List<ElementStackToken>>();
            CurrentRecipe = NullRecipe.Create();
        }
    }
}