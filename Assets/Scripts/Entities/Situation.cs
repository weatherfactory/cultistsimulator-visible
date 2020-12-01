using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.Core.NullObjects;
using Assets.Core.Services;
using Assets.Core.States;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Logic;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.TokenContainers;
using JetBrains.Annotations;
using Noon;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Core.Entities {

    public class Situation: ISphereEventSubscriber
    {
        public SituationState CurrentState { get; set; }
        public Recipe CurrentPrimaryRecipe { get; set; }
        public RecipePrediction CurrentRecipePrediction{ get; set; }

        public float TimeRemaining { set; get; }

        public float IntervalForLastHeartbeat { private set; get; }

        public float Warmup
        {
            get { return CurrentPrimaryRecipe.Warmup; }
        }

        public string RecipeId
        {
            get { return CurrentPrimaryRecipe == null ? null : CurrentPrimaryRecipe.Id; }
        }

        public virtual IVerb Verb { get; set; }
        private readonly List<ISituationSubscriber> subscribers = new List<ISituationSubscriber>();
        public HashSet<SituationInterruptInput> CurrentInterrupts = new HashSet<SituationInterruptInput>();
        private readonly HashSet<Sphere> _spheres = new HashSet<Sphere>();
        public string OverrideTitle { get; set; }

        private Token _anchor;
        private SituationWindow _window;
        
        public SituationPath Path { get; }
        public bool IsOpen { get; private set; }

        public virtual bool IsValidSituation()
        {
            return true;
        }


        public RecipeBeginningEffectCommand CurrentBeginningEffectCommand= new RecipeBeginningEffectCommand();
        public RecipeCompletionEffectCommand CurrentCompletionEffectCommand=new RecipeCompletionEffectCommand();


        public TokenLocation GetAnchorLocation()
        {
            return _anchor.Location;
        }

        public Token GetAnchor()
        {
            return _anchor;
        }

        public Vector3 GetWindowLocation()
        {
            return _window.positioner.GetPosition();
        }

        public const float HOUSEKEEPING_CYCLE_BEATS = 1f;

        public Situation()
        {

        }

        public Situation(SituationCreationCommand command)
        {
            Verb = command.GetBasicOrCreatedVerb();
            TimeRemaining = command.TimeRemaining ?? 0;
            CurrentPrimaryRecipe = command.Recipe;
            OverrideTitle = command.OverrideTitle;
            Path = command.SituationPath;
            CurrentBeginningEffectCommand.OngoingSlots=new List<SlotSpecification>(command.OngoingSlots);
            CurrentState = SituationState.Rehydrate(command.State,this);
        }



        public void AttachAnchor(Token newAnchor)
        {
            _anchor = newAnchor;
            AddSubscriber(_anchor);
            _anchor.OnWindowClosed.AddListener(Close);
            _anchor.OnStart.AddListener(TryStart);
            _anchor.OnCollect.AddListener(CollectOutputStacks);
            _anchor.OnContainerAdded.AddListener(AddContainer);
            _anchor.OnContainerRemoved.AddListener(RemoveContainer);

            _anchor.Populate(this);
        }



        public void AttachWindow(SituationWindow newWindow)
        {
            _window = newWindow;
            AddSubscriber(_window);


            _window.OnWindowClosed.AddListener(Close);
            _window.OnStart.AddListener(TryStart);
            _window.OnCollect.AddListener(CollectOutputStacks);
            _window.OnContainerAdded.AddListener(AddContainer);
            _window.OnContainerRemoved.AddListener(RemoveContainer);

            _window.Initialise(this);


        }


        public bool AddSubscriber(ISituationSubscriber subscriber)
        {

            if (subscribers.Contains(subscriber))
                return false;

            subscribers.Add(subscriber);
            return true;
        }

        public bool RemoveSubscriber(ISituationSubscriber subscriber)
        {
            if (!subscribers.Contains(subscriber))
                return false;

            subscribers.Remove(subscriber);
            return true;
        }


        public void AddContainer(Sphere container)
        {
            container.Subscribe(this);
            _spheres.Add(container);
        }

        public void AddContainers(IEnumerable<Sphere> containers)
        {
            foreach (var c in containers)
                AddContainer(c);
        }

        public void RemoveContainer(Sphere c)
        {
            c.Unsubscribe(this);
            _spheres.Remove(c);
            
        }

        public IList<SlotSpecification> GetSlotsForCurrentRecipe()
        {
            if (CurrentPrimaryRecipe.Slots.Any())
                return CurrentPrimaryRecipe.Slots;
            else
                return new List<SlotSpecification>();
        }


        public void Reset()
        {
            CurrentPrimaryRecipe = NullRecipe.Create(Verb);
            CurrentRecipePrediction = RecipePrediction.DefaultFromVerb(Verb);
            TimeRemaining = 0;
            }

        public void Halt()
        {
            CurrentInterrupts.Add(SituationInterruptInput.Halt);
        }

        public List<Sphere> GetSpheres()
        {
            return new List<Sphere>(_spheres);
        }


        public List<Sphere> GetSpheresActiveForCurrentState()
        {
            return new List<Sphere>(_spheres).Where(sphere => CurrentState.IsActiveInThisState(sphere)).ToList();
        }

        public List<Sphere> GetSpheresByCategory(SphereCategory category)
        {
            return new List<Sphere>(_spheres.Where(c => c.SphereCategory == category));
        }

        public Sphere GetSingleSphereByCategory(SphereCategory category)
        {
            try
            {
                return _spheres.SingleOrDefault(c => c.SphereCategory == category);
            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }

            try
            {
                return GetSpheresByCategory(category).First();
            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }

            return null;

        }
        
        public void Retire()
        {
            foreach (var c in _spheres)
            {
                c.Retire();
            }

            _window.Retire();
            _anchor.Retire();
            Registry.Get<SituationsCatalogue>().DeregisterSituation(this);
        }


        public void NotifyGreedySlotAnim(TokenAnimationToSlot slotAnim)
        {
//            greedyAnimIsActive = true;
            slotAnim.onElementSlotAnimDone += HandleOnGreedySlotAnimDone;


        }

        void HandleOnGreedySlotAnimDone(Token token, TokenLocation destination,
            Sphere destinatinoSlot)
        {
  //          greedyAnimIsActive = false;
        }


        public void ExecuteHeartbeat(float interval)
        {
            Continue(interval);
        }


        public int TryPurgeStacks(Element elementToPurge, int maxToPurge)
        {

            var containersToPurge = GetSpheresByCategory(SphereCategory.Threshold).ToList();

            containersToPurge
                .Reverse(); //I couldn't remember why I put this - but I think it must have been to start with the final slot, so we don't dump everything by purging the primary slot.


            containersToPurge.AddRange(GetSpheresByCategory(SphereCategory.Output));


            foreach (var container in containersToPurge)
            {
                if (maxToPurge <= 0)
                    return maxToPurge;
                else
                    maxToPurge -= container.TryPurgeStacks(elementToPurge, maxToPurge);
            }

            return maxToPurge;



        }

        
    public void AcceptToken(SphereCategory forSphereCategory, Token token, Context context)
        {
            var tokenList = new List<Token> {token};
            AcceptTokens(forSphereCategory, tokenList, context);
        }

        public void AcceptTokens(SphereCategory forSphereCategory, IEnumerable<Token> tokens,
            Context context)
        {
            var acceptingContainer = GetSingleSphereByCategory(forSphereCategory);
            acceptingContainer.AcceptTokens(tokens, context);
        }

        public void AcceptTokens(SphereCategory forSphereCategory, IEnumerable<Token> tokens)
        {
            AcceptTokens(forSphereCategory,tokens,new Context(Context.ActionSource.Unknown));
        }

        public List<ElementStack> GetAllStacksInSituation()
        {
            List<ElementStack> stacks = new List<ElementStack>();
            foreach (var container in _spheres)
                stacks.AddRange(container.GetElementTokens().Select(t=>t.ElementStack));
            return stacks;
        }

        public List<Token> GetTokens(SphereCategory forSphereCategory)
        {
            List<Token> stacks = new List<Token>();
            foreach (var container in _spheres.Where(c => c.SphereCategory == forSphereCategory))
                stacks.AddRange(container.GetAllTokens());

            return stacks;
        }

        public List<Token> GetElementTokens(SphereCategory forSphereCategory)
        {
            List<Token> stacks = new List<Token>();
            foreach (var container in _spheres.Where(c => c.SphereCategory == forSphereCategory))
                stacks.AddRange(container.GetElementTokens());

            return stacks;
        }

        public List<ElementStack> GetStacks(SphereCategory forSphereCategory)
        {
            List<ElementStack> stacks = new List<ElementStack>();
            foreach (var container in _spheres.Where(c => c.SphereCategory == forSphereCategory))
                stacks.AddRange(container.GetElementTokens().Select(s=>s.ElementStack));

            return stacks;
        }

        public IAspectsDictionary GetAspectsAvailableToSituation(bool includeElementAspects)
        {
            var aspects = new AspectsDictionary();

            foreach (var container in _spheres.Where(c =>
                c.SphereCategory == SphereCategory.SituationStorage ||
                c.SphereCategory == SphereCategory.Threshold ||
            c.SphereCategory == SphereCategory.Output))
            {
                aspects.CombineAspects(container.GetTotalAspects(includeElementAspects));
            }

            return aspects;

        }

        private SituationState Continue(float interval)
        {
            IntervalForLastHeartbeat = interval;

            CurrentState.Continue(this);


            CurrentBeginningEffectCommand = new RecipeBeginningEffectCommand();
            CurrentCompletionEffectCommand = new RecipeCompletionEffectCommand();

            return CurrentState;
        }


        public void NotifySubscribersOfSituationStateChange()
        {
            foreach (var subscriber in subscribers)
            {
                subscriber.SituationStateChanged(this);
            }
        }

        public void NotifySubscribersOfTimerValueUpdate()
        {
            foreach (var subscriber in subscribers)
                subscriber.TimerValuesChanged(this);
        }


        private void PossiblySignalImpendingDoom(EndingFlavour endingFlavour)
        {
            var tabletopManager = Registry.Get<TabletopManager>();
            if (endingFlavour != EndingFlavour.None)
                tabletopManager.SignalImpendingDoom(_anchor);
            else
                tabletopManager.NoMoreImpendingDoom(_anchor);

        }

        public void ExecuteCurrentRecipe()
        {
            
            var tc = Registry.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(GetAspectsAvailableToSituation(true));

            RecipeConductor rc =new RecipeConductor(aspectsInContext, Registry.Get<Character>());

            IList<RecipeExecutionCommand> recipeExecutionCommands = rc.GetRecipeExecutionCommands(CurrentPrimaryRecipe);

            //actually replace the current recipe with the first on the list: any others will be additionals,
            //but we want to loop from this one.
            if (recipeExecutionCommands.First().Recipe.Id != CurrentPrimaryRecipe.Id)
                CurrentPrimaryRecipe = recipeExecutionCommands.First().Recipe;


            foreach (var container in _spheres)
                container
                    .ActivatePreRecipeExecutionBehaviour(); //currently, this is just marking items in slots for consumption, so it happens once for everything. I might later want to run it per effect command, tho
            //on the other hand, I *do* want this to run before the stacks are moved to storage.
            //but finally, it would probably do better in AcceptToken on the recipeslot anyway

            //move any elements currently in OngoingSlots to situation storage
            //NB we're doing this *before* we execute the command - the command may affect these elements too
            GetSingleSphereByCategory(SphereCategory.SituationStorage).AcceptTokens(
                GetTokens(SphereCategory.Threshold), new Context(Context.ActionSource.SituationStoreStacks));


            foreach (var c in recipeExecutionCommands)
            {
                RecipeCompletionEffectCommand currentEffectCommand = new RecipeCompletionEffectCommand(c.Recipe,
                    c.Recipe.ActionId != CurrentPrimaryRecipe.ActionId, c.Expulsion,c.ToPath);
                if (currentEffectCommand.AsNewSituation)
                    CreateNewSituation(currentEffectCommand);
                else
                {
                    Registry.Get<Character>().AddExecutionsToHistory(currentEffectCommand.Recipe.Id, 1); //can we make 
                    var executor = new SituationEffectExecutor(Registry.Get<TabletopManager>());
                    executor.RunEffects(currentEffectCommand, GetSingleSphereByCategory(SphereCategory.SituationStorage), Registry.Get<Character>(), Registry.Get<IDice>());

                    if (!string.IsNullOrEmpty(currentEffectCommand.Recipe.Ending))
                    {
                        var ending = Registry.Get<Compendium>().GetEntityById<Ending>(currentEffectCommand.Recipe.Ending);
                        Registry.Get<TabletopManager>() .EndGame(ending, this._anchor); //again, ttm (or successor) is subscribed. We should do it through there.
                    }
                    
                    
                    TryOverrideVerbIcon(GetAspectsAvailableToSituation(true));


                }

            }
        }
        //TODO: I don't love this - it goes outside the subscription flow - but there's an expensive lookup here. I should cache it and then rationalise.
        private void TryOverrideVerbIcon(IAspectsDictionary forAspects)
        {
            //if we have an element in the situation now that overrides the verb icon, update it
            string overrideIcon = Registry.Get<Compendium>().GetVerbIconOverrideFromAspects(forAspects);
            if (!string.IsNullOrEmpty(overrideIcon))
            {
                _anchor.DisplayOverrideIcon(overrideIcon);
                _window.DisplayIcon(overrideIcon);
            }
        }
        private void CreateNewSituation(RecipeCompletionEffectCommand effectCommand)
        {
            List<Token> stacksToAddToNewSituation = new List<Token>();
            //if there's an expulsion
            if (effectCommand.Expulsion.Limit > 0)
            {
                //find one or more matching stacks. Important! the limit applies to stacks, not cards. This might need to change.
                AspectMatchFilter filter = new AspectMatchFilter(effectCommand.Expulsion.Filter);
                var filteredStacks = filter.FilterElementStacks(GetElementTokens(SphereCategory.SituationStorage)).ToList();

                if (filteredStacks.Any() && effectCommand.Expulsion.Limit > 0)
                {
                    while (filteredStacks.Count > effectCommand.Expulsion.Limit)
                    {
                        filteredStacks.RemoveAt(filteredStacks.Count - 1);
                    }

                    stacksToAddToNewSituation = filteredStacks;
                }

            }




            IVerb verbForNewSituation = Registry.Get<Compendium>().GetVerbForRecipe(effectCommand.Recipe);


            TokenLocation newAnchorLocation;

            if (effectCommand.ToPath != SpherePath.Current())
                newAnchorLocation = new TokenLocation(Vector3.zero, effectCommand.ToPath);
            else
                newAnchorLocation = _anchor.Location;


            var scc = new SituationCreationCommand(verbForNewSituation, effectCommand.Recipe,
                StateEnum.Unstarted, newAnchorLocation, _anchor);
            var newSituation=Registry.Get<SituationsCatalogue>()
                .TryBeginNewSituation(scc,
                    stacksToAddToNewSituation); //tabletop manager is a subscriber, right? can we run this (or access to its successor) through that flow?

            newSituation.TryStart();

        }

    public void SendNotificationToSubscribers(INotification notification)
    {
        //Check for possible text refinements based on the aspects in context
        var aspectsInSituation = GetAspectsAvailableToSituation(true);
        TextRefiner tr = new TextRefiner(aspectsInSituation);


        Notification refinedNotification = new Notification(notification.Title,
            tr.RefineString(notification.Description));
        
            foreach (var subscriber in subscribers)
                subscriber.ReceiveNotification(refinedNotification);

        }



    public void AttemptAspectInductions(Recipe currentRecipe, List<Token> outputTokens) // this should absolutely go through subscription - something to succeed ttm
    {
        //If any elements in the output, or in the situation itself, have inductions, test whether to start a new recipe

     var inducingAspects = new AspectsDictionary();

        //shrouded cards don't trigger inductions. This is because we don't generally want to trigger an induction
        //for something that has JUST BEEN CREATED. This started out as a hack, but now we've moved from 'face-down'
        //to 'shrouded' it feels more suitable.

        foreach (var os in outputTokens)
        {
            if (!os.Shrouded())
                inducingAspects.CombineAspects(os.ElementStack.GetAspects());
        }


        inducingAspects.CombineAspects(currentRecipe.Aspects);


        foreach (var a in inducingAspects)
        {
            var aspectElement = Registry.Get<Compendium>().GetEntityById<Element>(a.Key);

            if (aspectElement != null)
                PerformAspectInduction(aspectElement);
            else
                NoonUtility.Log("unknown aspect " + a + " in output");
        }
    }



    void PerformAspectInduction(Element aspectElement)
    {
        foreach (var induction in aspectElement.Induces)
        {
            var d = Registry.Get<IDice>();

            if (d.Rolld100() <= induction.Chance)
                CreateRecipeFromInduction(Registry.Get<Compendium>() .GetEntityById<Recipe>(induction.Id), aspectElement.Id);
        }
    }

    void CreateRecipeFromInduction(Recipe inducedRecipe, string aspectID) // yeah this *definitely* should be through subscription!
    {
        if (inducedRecipe == null)
        {
            NoonUtility.Log("unknown recipe " + inducedRecipe + " in induction for " + aspectID);
            return;
        }
        
            var inductionRecipeVerb = Registry.Get<Compendium>().GetVerbForRecipe(inducedRecipe);
            SituationCreationCommand inducedSituationCreationCommand = new SituationCreationCommand(inductionRecipeVerb,
            inducedRecipe, StateEnum.Unstarted, _anchor.Location, _anchor);
        var inducedSituation=Registry.Get<SituationsCatalogue>().TryBeginNewSituation(inducedSituationCreationCommand, new List<Token>());
            inducedSituation.TryStart();

    }


        public void Close()
        { 
            IsOpen = false; 

            _window.Hide(this);

            DumpUnstartedBusiness();
        }


    public void OpenAt(TokenLocation location)
    {
        IsOpen = true;
        _window.Show(location.Position,this);
            
        Registry.Get<TabletopManager>().CloseAllSituationWindowsExcept(_anchor.Verb.Id);
    }

    public virtual void OpenAtCurrentLocation()
    {
        var currentLocation = GetAnchorLocation();
        if(currentLocation==null)
            OpenAt(new TokenLocation(Vector3.zero,currentLocation.AtSpherePath));
        else
            OpenAt(currentLocation);
    }



    public Sphere GetFirstAvailableThresholdForStackPush(ElementStack stack)
    {
        var thresholds = GetSpheresByCategory(SphereCategory.Threshold);
        foreach (var t in thresholds)

            if (!t.IsGreedy && !t.CurrentlyBlockedFor(BlockDirection.Inward) && t.GetMatchForStack(stack).MatchType ==SlotMatchForAspectsType.Okay)
                return t;

        return Registry.Get<NullSphere>();
    }


        public bool PushDraggedStackIntoThreshold(Token token)
        {
            var thresholdContainer = GetFirstAvailableThresholdForStackPush(token.ElementStack);
            var possibleIncumbent = thresholdContainer.GetElementTokens().FirstOrDefault();
            if (possibleIncumbent != null)
            {
                possibleIncumbent.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
            }

            return  thresholdContainer.TryAcceptTokenAsThreshold(token);

        }



        public void DumpUnstartedBusiness()
        {
       //deferring this a bit longer. I need to think about how to connect startingslot behaviour with the BOH model:
       //slot behaviour: dump when window closed?
       //slot behaviour: block for certain kinds of interaction? using existing block?
       //slot behaviour: specify connection type with other containers? ie expand 'greedy' effect to mean multiple things and directions
            //if(CurrentState!=CurrentState.Ongoing)
            //{
            //    var slotted = GetStacks(ContainerCategory.Threshold);
            //    foreach (var item in slotted)
            //        item.ReturnToTabletop(new Context(Context.ActionSource.PlayerDumpAll));
            //}


        }

        public void CollectOutputStacks()
        {
            var results = GetElementTokens(SphereCategory.Output);
            foreach (var item in results)
                item.ReturnToTabletop(new Context(Context.ActionSource.PlayerDumpAll));

            Continue(0f);

            // Only play collect all if there's actually something to collect 
            // Only play collect all if it's not transient - cause that will retire it and play the retire sound
            // Note: If we collect all from the window we also get the default button sound in any case.
            if (results.Any())
                SoundManager.PlaySfx("SituationCollectAll");
            else if (_anchor.Durability == AnchorDurability.Transient)
                SoundManager.PlaySfx("SituationTokenRetire");
            else
                SoundManager.PlaySfx("UIButtonClick");
        }

        public void TryDecayOutputContents( float interval)
        {
            var stacksToDecay = GetStacks(SphereCategory.Output);
            foreach (var s in stacksToDecay)
                s.Decay(interval);
        }


        public void TryStart()
        {
         
            var aspects = GetAspectsAvailableToSituation(true);
            var tc = Registry.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(aspects);


            var recipe = Registry.Get<Compendium>().GetFirstMatchingRecipe(aspectsInContext, Verb.Id, Registry.Get<Character>(), false);

            //no recipe found? get outta here
            if (recipe != null)

            {
            
                CurrentInterrupts.Add(SituationInterruptInput.Start);

                CurrentPrimaryRecipe = recipe;
                TimeRemaining = CurrentPrimaryRecipe.Warmup;

                //The game might be paused! or the player might just be incredibly quick off the mark
                //so immediately continue with a 0 interval - this won't advance time, but will update the visuals in the situation window
                //(which among other things should make the starting slot unavailable

                Continue(0f);
            }

        }


        public RecipePrediction GetUpdatedRecipePrediction()
        {
            var aspectsAvailableToSituation = GetAspectsAvailableToSituation(true);

            var aspectsInContext =
                Registry.Get<SphereCatalogue>().GetAspectsInContext(aspectsAvailableToSituation);

            RecipeConductor rc = new RecipeConductor(aspectsInContext,Registry.Get<Character>());

            return rc.GetPredictionForFollowupRecipe(CurrentPrimaryRecipe, this);
        }

        public void NotifyTokensChangedForSphere(TokenInteractionEventArgs args)
        {

            CurrentRecipePrediction = GetUpdatedRecipePrediction();
            PossiblySignalImpendingDoom(CurrentRecipePrediction.SignalEndingFlavour);

            foreach (var s in subscribers)
                s.SphereContentsUpdated(this);


        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
//
        }


    }

}
