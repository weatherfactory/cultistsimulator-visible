using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Commands;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.States;
using SecretHistories.UI;
using Assets.Logic;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;
using JetBrains.Annotations;
using SecretHistories.Core;
using SecretHistories.States.TokenStates;
using UnityEngine;
using UnityEngine.Assertions;

namespace SecretHistories.Entities {

    [IsEncaustableClass(typeof(SituationCreationCommand))]
    public class Situation: ISphereEventSubscriber
    {
        [Encaust]
        public SituationState CurrentState { get; set; }

        [Encaust]
        public Recipe Recipe { get; set; }
        
        [Encaust]
        public float TimeRemaining { set; get; }

        [Encaust]
        public float IntervalForLastHeartbeat { private set; get; }

        [Encaust]
        public virtual IVerb Verb { get; set; }

        [Encaust]
        public string OverrideTitle { get; set; }

        [Encaust]
        public SituationPath Path { get; }

        [Encaust]
        public bool IsOpen { get; private set; }

        [Encaust]
        public SituationCommandQueue CommandQueue { get; set; } = new SituationCommandQueue();

        [Encaust]
        public RecipeCompletionEffectCommand CurrentCompletionEffectCommand { get; set; } = new RecipeCompletionEffectCommand();

        [Encaust]
        public TokenLocation AnchorLocation => _anchor.Location;

        [DontEncaust] public float Warmup => Recipe.Warmup;
        [DontEncaust] public string RecipeId => Recipe.Id;
        [DontEncaust] public RecipePrediction CurrentRecipePrediction { get; set; }

    
        private readonly List<ISituationSubscriber> _subscribers = new List<ISituationSubscriber>();
        private readonly List<Dominion> _registeredDominions = new List<Dominion>();
        private readonly HashSet<Sphere> _spheres = new HashSet<Sphere>();
        private Token _anchor;
        private SituationWindow _window;

        

        public Token GetAnchor()
        {
            return _anchor;
        }

        public Vector3 GetWindowLocation()
        {
            return _window.positioner.GetPosition();
        }


        public Situation(SituationPath path)
        {
            Path = path;
            Recipe = NullRecipe.Create(NullVerb.Create());
        }

        public void TransitionToState(SituationState newState)
        {
            CurrentState.Exit(this);
            newState.Enter(this);
            CurrentState = newState;
            NotifySubscribersOfStateAndTimerChange();
        }

        public void Attach(Token newAnchor)
        {
            _anchor = newAnchor;
            AddSubscriber(_anchor);
            _anchor.OnWindowClosed.AddListener(Close);
            _anchor.OnStart.AddListener(TryStart);
            _anchor.OnCollect.AddListener(Conclude);
            _anchor.OnSphereAdded.AddListener(AttachSphere);
            _anchor.OnSphereRemoved.AddListener(RemoveSphere);
            _anchor.AttachedTo(this);
            NotifySubscribersOfStateAndTimerChange();
            NotifySubscribersOfTimerValueUpdate();
        }



        public void Attach(SituationWindow newWindow)
        {
            _window = newWindow;
            AddSubscriber(_window);

            _window.OnWindowClosed.AddListener(Close);
            _window.OnStart.AddListener(TryStart);
            _window.OnCollect.AddListener(Conclude);
            _window.OnSphereAdded.AddListener(AttachSphere);
            _window.OnSphereRemoved.AddListener(RemoveSphere);

            _window.Initialise(this);
            _window.positioner.Initialise(_anchor);
            NotifySubscribersOfStateAndTimerChange();
            NotifySubscribersOfTimerValueUpdate();
        }

        public bool RegisterDominion(Dominion dominionToRegister)
        {
            AddSubscriber(dominionToRegister);
            dominionToRegister.OnSphereAdded.AddListener(AttachSphere);
            dominionToRegister.OnSphereRemoved.AddListener(RemoveSphere);

            if (_registeredDominions.Contains(dominionToRegister))
                return false;

            _registeredDominions.Add(dominionToRegister);
            return true;
        }

        public bool AddSubscriber(ISituationSubscriber subscriber)
        {

            if (_subscribers.Contains(subscriber))
                return false;

            _subscribers.Add(subscriber);
            return true;
        }

        public bool RemoveSubscriber(ISituationSubscriber subscriber)
        {
            if (!_subscribers.Contains(subscriber))
                return false;

            _subscribers.Remove(subscriber);
            return true;
        }


        public void AttachSphere(Sphere container)
        {
            container.Subscribe(this);
            _spheres.Add(container);
        }

        public void AttachSpheres(IEnumerable<Sphere> containers)
        {
            foreach (var c in containers)
                AttachSphere(c);
        }

        public void RemoveSphere(Sphere c)
        {
            c.Unsubscribe(this);
            _spheres.Remove(c);
            
        }

        public void Reset()
        {
            Recipe = NullRecipe.Create(Verb);
            CurrentRecipePrediction = RecipePrediction.DefaultFromVerb(Verb);
            TimeRemaining = 0;
            NotifySubscribersOfStateAndTimerChange();
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
                c.Retire(SphereRetirementType.Destructive);
            }

            _window.Retire();
            _anchor.Retire();
            Watchman.Get<SituationsCatalogue>().DeregisterSituation(this);
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
            var acceptingSphere = GetSingleSphereByCategory(forSphereCategory);
            acceptingSphere.AcceptTokens(tokens, context);
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

            CurrentCompletionEffectCommand = new RecipeCompletionEffectCommand();

            return CurrentState;
        }


        public void NotifySubscribersOfStateAndTimerChange()
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.SituationStateChanged(this);
                subscriber.TimerValuesChanged(this);
            }
        }

        public void NotifySubscribersOfTimerValueUpdate()
        {
            foreach (var subscriber in _subscribers)
                subscriber.TimerValuesChanged(this);
        }


        private void PossiblySignalImpendingDoom(EndingFlavour endingFlavour)
        {
            var tabletopManager = Watchman.Get<TabletopManager>();
            if (endingFlavour != EndingFlavour.None)
                tabletopManager.SignalImpendingDoom(_anchor);
            else
                tabletopManager.NoMoreImpendingDoom(_anchor);

        }

        public void ExecuteCurrentRecipe()
        {
            
            var tc = Watchman.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(GetAspectsAvailableToSituation(true));

            RecipeConductor rc =new RecipeConductor(aspectsInContext, Watchman.Get<Character>());

            IList<RecipeExecutionCommand> recipeExecutionCommands = rc.GetRecipeExecutionCommands(Recipe);

            //actually replace the current recipe with the first on the list: any others will be additionals,
            //but we want to loop from this one.
            if (recipeExecutionCommands.First().Recipe.Id != Recipe.Id)
                Recipe = recipeExecutionCommands.First().Recipe;


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
                    c.Recipe.ActionId != Recipe.ActionId, c.Expulsion,c.ToPath);
                if (currentEffectCommand.AsNewSituation)
                    CreateNewSituation(currentEffectCommand);
                else
                {
                    Watchman.Get<Character>().AddExecutionsToHistory(currentEffectCommand.Recipe.Id, 1); //can we make 
                    var executor = new SituationEffectExecutor(Watchman.Get<TabletopManager>());
                    executor.RunEffects(currentEffectCommand, GetSingleSphereByCategory(SphereCategory.SituationStorage), Watchman.Get<Character>(), Watchman.Get<IDice>());

                    if (!string.IsNullOrEmpty(currentEffectCommand.Recipe.Ending))
                    {
                        var ending = Watchman.Get<Compendium>().GetEntityById<Ending>(currentEffectCommand.Recipe.Ending);
                        Watchman.Get<TabletopManager>() .EndGame(ending, this._anchor); //again, ttm (or successor) is subscribed. We should do it through there.
                    }
                    
                    
                    TryOverrideVerbIcon(GetAspectsAvailableToSituation(true));


                }

            }
        }
        //TODO: I don't love this - it goes outside the subscription flow - but there's an expensive lookup here. I should cache it and then rationalise.
        private void TryOverrideVerbIcon(IAspectsDictionary forAspects)
        {
            //if we have an element in the situation now that overrides the verb icon, update it
            string overrideIcon = Watchman.Get<Compendium>().GetVerbIconOverrideFromAspects(forAspects);
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




            IVerb verbForNewSituation = Watchman.Get<Compendium>().GetVerbForRecipe(effectCommand.Recipe);


            TokenLocation newAnchorLocation;

            if (effectCommand.ToPath != SpherePath.Current())
                newAnchorLocation = new TokenLocation(Vector3.zero, effectCommand.ToPath);
            else
                newAnchorLocation = _anchor.Location;


            var scc = new SituationCreationCommand(verbForNewSituation, effectCommand.Recipe,
                StateEnum.Unstarted, newAnchorLocation).WithDefaultAttachments();

            scc.SourceToken = _anchor;

            var newSituation=Watchman.Get<SituationsCatalogue>()
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
        
            foreach (var subscriber in _subscribers)
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
            var aspectElement = Watchman.Get<Compendium>().GetEntityById<Element>(a.Key);

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
            var d = Watchman.Get<IDice>();

            if (d.Rolld100() <= induction.Chance)
                CreateRecipeFromInduction(Watchman.Get<Compendium>() .GetEntityById<Recipe>(induction.Id), aspectElement.Id);
        }
    }

    void CreateRecipeFromInduction(Recipe inducedRecipe, string aspectID) // yeah this *definitely* should be through subscription!
    {
        if (inducedRecipe == null)
        {
            NoonUtility.Log("unknown recipe " + inducedRecipe + " in induction for " + aspectID);
            return;
        }
        
            var inductionRecipeVerb = Watchman.Get<Compendium>().GetVerbForRecipe(inducedRecipe);
            SituationCreationCommand inducedSituationCreationCommand = new SituationCreationCommand(inductionRecipeVerb,
            inducedRecipe, StateEnum.Unstarted, _anchor.Location).WithDefaultAttachments();

            inducedSituationCreationCommand.SourceToken = _anchor;

        var inducedSituation=Watchman.Get<SituationsCatalogue>().TryBeginNewSituation(inducedSituationCreationCommand, new List<Token>());
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
        _window.Show(location.Anchored3DPosition,this);
            
        Watchman.Get<TabletopManager>().CloseAllSituationWindowsExcept(_anchor.Payload.Id);
    }

    public virtual void OpenAtCurrentLocation()
    {
        var currentLocation = AnchorLocation;
        if(currentLocation==null)
            OpenAt(new TokenLocation(Vector3.zero,currentLocation.AtSpherePath));
        else
            OpenAt(currentLocation);
    }



    public List<Dominion> GetSituationDominionsForCommandCategory(CommandCategory commandCategory)
    {
            return new List<Dominion>(_registeredDominions.Where(a=>a.MatchesCommandCategory(commandCategory)));
    }

    public List<Sphere> GetAvailableThresholdsForStackPush(ElementStack stack)
    {
        List<Sphere> thresholdsAvailable=new List<Sphere>();
        var thresholds = GetSpheresByCategory(SphereCategory.Threshold);
        foreach (var t in thresholds)

            if (!t.IsGreedy
               && CurrentState.IsActiveInThisState(t)
               && !t.CurrentlyBlockedFor(BlockDirection.Inward)
               && t.GetMatchForStack(stack).MatchType ==SlotMatchForAspectsType.Okay)

                thresholdsAvailable.Add(t);

        return thresholdsAvailable;

    }

    public void InteractWithSituation(Token incomingToken)
    {

        if (incomingToken.ElementStack.IsValidElementStack())
        {
            TryPushDraggedStackIntoThreshold(incomingToken);
        }
        else
        {
                //something has gone awryy
                incomingToken.SetState(new RejectedBySituationState());
        }
    }


        public bool TryPushDraggedStackIntoThreshold(Token token)
        {
            var thresholdSpheres = GetAvailableThresholdsForStackPush(token.ElementStack);
            if (thresholdSpheres.Count <= 0)
                return false;

            Sphere emptyThresholdSphere = thresholdSpheres.FirstOrDefault(t => t.IsEmpty());

            if(emptyThresholdSphere!=null)
                return emptyThresholdSphere.TryAcceptToken(token,new Context(Context.ActionSource.PlayerDrag));

            Sphere occupiedThresholdSphere = thresholdSpheres.FirstOrDefault();
            if(occupiedThresholdSphere!=null)
            {
                var incumbent = occupiedThresholdSphere.GetElementTokens().FirstOrDefault();
                if(incumbent!=null) //should never happen, but code might shift later
                    incumbent.GoAway(new Context(Context.ActionSource.PushToThresholddUsurpedThisStack));
                 
                return occupiedThresholdSphere.TryAcceptToken(token, new Context(Context.ActionSource.PlayerDrag));
            }


            return false;

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
        
        /// <summary>
        /// collects output stacks, retires transient verbs (or should)
        /// </summary>
        public void Conclude()
        {
           CommandQueue.AddCommand(new ConcludeCommand());
            Continue(0f);
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
            var tc = Watchman.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(aspects);


            var recipe = Watchman.Get<Compendium>().GetFirstMatchingRecipe(aspectsInContext, Verb.Id, Watchman.Get<Character>(), false);

            //no recipe found? get outta here
            if (recipe != null)

            {
               var activateRecipeCommand=new TryActivateRecipeCommand(recipe);
                CommandQueue.AddCommand(activateRecipeCommand);

                //The game might be paused! or the player might just be incredibly quick off the mark
                //so immediately continue with a 0 interval

                Continue(0f);
            }

        }


        public RecipePrediction GetUpdatedRecipePrediction()
        {
            var aspectsAvailableToSituation = GetAspectsAvailableToSituation(true);

            var aspectsInContext =
                Watchman.Get<SphereCatalogue>().GetAspectsInContext(aspectsAvailableToSituation);

            RecipeConductor rc = new RecipeConductor(aspectsInContext,Watchman.Get<Character>());

            return rc.GetPredictionForFollowupRecipe(Recipe, this);
        }



        public bool ForbidCreationOf(SituationCreationCommand scc)
        {
            if (scc.Verb.Id != Verb.Id)
                return false;

            if (scc.Verb.Transient && CurrentState.AllowDuplicateVerbIfTransient) //doesn't matter whether ID matches or not
                return false;

            return true;
        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            if(args.Context.IsManualAction() &&  !IsOpen)
                OpenAtCurrentLocation();
            
            
            CurrentRecipePrediction = GetUpdatedRecipePrediction();
            PossiblySignalImpendingDoom(CurrentRecipePrediction.SignalEndingFlavour);

            foreach (var s in _subscribers)
                s.SituationSphereContentsUpdated(this);
        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            //
        }
        
        public virtual bool IsValidSituation()
        {
            return true;
        }
    }

}
