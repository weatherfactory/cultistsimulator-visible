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
        public Recipe PredictedRecipe;

        public Dictionary<ContainerCategory, List<ElementStackToken>> StacksInEachStorage { get; set; }
        public INotification Notification;
        public SituationEffectCommand EffectCommand;
        public SituationState SituationState;

        public static SituationEventData Create(Situation fromSituation)
        {
            var e = new SituationEventData();
            e.Warmup = fromSituation.Warmup;
            e.TimeRemaining = fromSituation.TimeRemaining;
            e.CurrentRecipe = fromSituation.currentPrimaryRecipe;
            e.PredictedRecipe = fromSituation.currentPredictedRecipe;
            e.StacksInEachStorage.Add(ContainerCategory.Threshold,fromSituation.GetStacks(ContainerCategory.Threshold));
            e.StacksInEachStorage.Add(ContainerCategory.SituationStorage, fromSituation.GetStacks(ContainerCategory.SituationStorage));
            e.StacksInEachStorage.Add(ContainerCategory.Output, fromSituation.GetStacks(ContainerCategory.Output));
            e.ActiveVerb = fromSituation.Verb;
            e.SituationState = fromSituation.State;
            return e;

        }

        private SituationEventData()
        {
            StacksInEachStorage=new Dictionary<ContainerCategory, List<ElementStackToken>>();
            CurrentRecipe = NullRecipe.Create();
            EffectCommand=new SituationEffectCommand();
        }
    }
}