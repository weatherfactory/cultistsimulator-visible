using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.Core.Services;
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

    public class Situation
    {
        public SituationState State { get; set; }
        public Recipe currentPrimaryRecipe { get; set; }
        public Recipe currentPredictedRecipe { get; set; }

        public float TimeRemaining { private set; get; }

        public float Warmup
        {
            get { return currentPrimaryRecipe.Warmup; }
        }

        public string RecipeId
        {
            get { return currentPrimaryRecipe == null ? null : currentPrimaryRecipe.Id; }
        }

        public readonly IVerb Verb;
        private readonly List<ISituationSubscriber> subscribers = new List<ISituationSubscriber>();
        private readonly HashSet<TokenContainer> _containers = new HashSet<TokenContainer>();
        public string OverrideTitle { get; set; }
        public int CompletionCount { get; set; }

        private ISituationAnchor _anchor;
        private SituationWindow _window;
        private bool greedyAnimIsActive;
        public string Path { get; }


        public TokenLocation GetAnchorLocation()
        {
            return _anchor.Location;
        }

        public TokenLocation GetWindowLocation()
        {
            return _window.Location;
        }

        public const float HOUSEKEEPING_CYCLE_BEATS = 1f;


        public Situation(SituationCreationCommand command)
        {
            Verb = command.GetBasicOrCreatedVerb();
            TimeRemaining = command.TimeRemaining ?? 0;
            State = command.State;
            currentPrimaryRecipe = command.Recipe;
            OverrideTitle = command.OverrideTitle;
            CompletionCount = command.CompletionCount;
            Path = command.Path;


        }



        public void AttachAnchor(ISituationAnchor newAnchor)
        {
            _anchor = newAnchor;
            AddSubscriber(_anchor);
            _anchor.Populate(this);
        }



        public void AttachWindow(SituationWindow newWindow)
        {
            _window = newWindow;
            AddSubscriber(_window);


            _window.OnWindowClosed.AddListener(Close);
            _window.OnStart.AddListener(ActivateRecipe);
            _window.OnCollect.AddListener(CollectOutputStacks);
            _window.OnContainerAdded.AddListener(AddContainer);
            _window.OnContainerRemoved.AddListener(RemoveContainer);

            _window.Populate(this);

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


        public void AddContainer(TokenContainer container)
        {
            _containers.Add(container);
            container.OnStacksChanged.AddListener(ContainerStacksChanged);
        }

        public void AddContainers(IEnumerable<TokenContainer> containers)
        {
            foreach (var c in containers)
                AddContainer(c);
        }

        public void RemoveContainer(TokenContainer c)
        {
            _containers.Remove(c);
            c.OnStacksChanged.RemoveListener(ContainerStacksChanged);
        }

        public IList<SlotSpecification> GetSlotsForCurrentRecipe()
        {
            if (currentPrimaryRecipe.Slots.Any())
                return currentPrimaryRecipe.Slots;
            else
                return new List<SlotSpecification>();
        }


        private void Reset()
        {
            currentPrimaryRecipe = NullRecipe.Create(Verb);
            currentPredictedRecipe = currentPrimaryRecipe;
            TimeRemaining = 0;
            State = SituationState.Unstarted;
            foreach (var subscriber in subscribers)
                subscriber.ResetSituation();
        }

        public void Halt()
        {
            if (State != SituationState.Complete && State != SituationState.ReadyToReset &&
                State != SituationState.Unstarted
            ) //don't halt if the situation is not running. This is not only superfluous but dangerous: 'complete' called from an already completed verb has bad effects
                Complete();

            //If we leave anything in the ongoing slot, it's lost, and also the situation ends up in an anomalous state which breaks loads
            AcceptStacks(ContainerCategory.SituationStorage, GetStacks(ContainerCategory.Threshold));

        }


        private IEnumerable<TokenContainer> GetContainersByCategory(ContainerCategory category)
        {
            return _containers.Where(c => c.ContainerCategory == category);
        }

        private TokenContainer GetSingleContainerByCategory(ContainerCategory category)
        {
            try
            {
                return _containers.SingleOrDefault(c => c.ContainerCategory == category);
            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }

            try
            {
                return GetContainersByCategory(category).First();
            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }

            return null;

        }

        public ElementStackToken ReprovisionElementStack(ContainerCategory containerCategory,
            StackCreationCommand stackCreationCommand, Source stackSource, string locatorid = null)
        {
            //containercategory works fine for storage, because there's only ever one, but I think we should pass something solid as locatorid / TokenLocation combined

            var containersInCategory = GetContainersByCategory(containerCategory);
            int containerCount = containersInCategory.Count();
            if (containerCount > 1)
            {
                NoonUtility.LogWarning(
                    $"Trying to reprovision a stack of {stackCreationCommand?.ElementId} in situation with verb {Verb?.Id} but found more than one container in category {containerCategory}");
                return null;
            }

            if (containerCount == 0)
            {
                NoonUtility.LogWarning(
                    $"Trying to reprovision a stack of {stackCreationCommand?.ElementId} in situation with verb {Verb?.Id} but can't find a container of category {containerCategory}");
                return null;
            }

            var reprovisionedStack = containersInCategory.First()
                .ProvisionStackFromCommand(stackCreationCommand, stackSource,
                    new Context(Context.ActionSource.Loading));

            return reprovisionedStack;
        }

        public void Retire()
        {
            foreach (var c in _containers)
            {
                c.Retire();
            }

            _window.Retire();
            _anchor.Retire();
            Registry.Get<SituationsCatalogue>().DeregisterSituation(this);
        }


        public void NotifyGreedySlotAnim(TokenAnimationToSlot slotAnim)
        {
            greedyAnimIsActive = true;
            slotAnim.onElementSlotAnimDone += HandleOnGreedySlotAnimDone;

            TabletopManager.RequestNonSaveableState(TabletopManager.NonSaveableType.Greedy, true);

        }

        void HandleOnGreedySlotAnimDone(ElementStackToken element, TokenLocation destination,
            TokenContainer destinatinoSlot)
        {
            greedyAnimIsActive = false;
            TabletopManager.RequestNonSaveableState(TabletopManager.NonSaveableType.Greedy, false);
        }


        public HeartbeatResponse ExecuteHeartbeat(float interval)
        {

            var ttm = Registry.Get<TabletopManager>();
            var aspectsInContext = ttm.GetAspectsInContext(GetAspectsAvailableToSituation(true));

            RecipeConductor rc = new RecipeConductor(Registry.Get<ICompendium>(),
                aspectsInContext, Registry.Get<IDice>(), Registry.Get<Character>());

            Continue(rc, interval, greedyAnimIsActive);

            if (State == SituationState.Ongoing)
                return GetResponseWithUnfilledGreedyThresholdsForThisSituation();

            return new HeartbeatResponse();

        }


        private HeartbeatResponse GetResponseWithUnfilledGreedyThresholdsForThisSituation()
        {
            var response = new HeartbeatResponse();

            if (TimeRemaining > HOUSEKEEPING_CYCLE_BEATS)
            {

                var greedyThresholds =
                    _containers.Where(c =>
                        c.ContainerCategory == ContainerCategory.Threshold && c.IsGreedy &&
                        c.GetTotalStacksCount() == 0);

                foreach (var g in greedyThresholds)
                {
                    var tokenAndSlot = new AnchorAndSlot {Token = _anchor, Threshold = g};
                    response.SlotsToFill.Add(tokenAndSlot);

                }
            }

            return response;
        }

        public int TryPurgeStacks(Element elementToPurge, int maxToPurge)
        {

            var containersToPurge = GetContainersByCategory(ContainerCategory.Threshold).ToList();

            containersToPurge
                .Reverse(); //I couldn't remember why I put this - but I think it must have been to start with the final slot, so we don't dump everything by purging the primary slot.


            containersToPurge.AddRange(GetContainersByCategory(ContainerCategory.Output));


            foreach (var container in containersToPurge)
            {
                if (maxToPurge <= 0)
                    return maxToPurge;
                else
                    maxToPurge -= container.TryPurgeStacks(elementToPurge, maxToPurge);
            }

            return maxToPurge;



        }

        
    public void AcceptStack(ContainerCategory forContainerCategory, ElementStackToken stackToken, Context context)
        {
            var stackTokenList = new List<ElementStackToken> {stackToken};
            AcceptStacks(forContainerCategory, stackTokenList, context);
        }

        public void AcceptStacks(ContainerCategory forContainerCategory, IEnumerable<ElementStackToken> stacks,
            Context context)
        {
            var acceptingContainer = GetSingleContainerByCategory(forContainerCategory);
            acceptingContainer.AcceptStacks(stacks, context);
        }

        public void AcceptStacks(ContainerCategory forContainerCategory, IEnumerable<ElementStackToken> stacks)
        {
            AcceptStacks(forContainerCategory,stacks,new Context(Context.ActionSource.Unknown));
        }

        public List<ElementStackToken> GetAllStacksInSituation()
        {
            List<ElementStackToken> stacks = new List<ElementStackToken>();
            foreach (var container in _containers)
                stacks.AddRange(container.GetStacks());
            return stacks;
        }


        public List<ElementStackToken> GetStacks(ContainerCategory forContainerCategory)
        {
            List<ElementStackToken> stacks = new List<ElementStackToken>();
            foreach (var container in _containers.Where(c => c.ContainerCategory == forContainerCategory))
                stacks.AddRange(container.GetStacks());

            return stacks;
        }

        public IAspectsDictionary GetAspectsAvailableToSituation(bool includeElementAspects)
        {
            var aspects = new AspectsDictionary();

            foreach (var container in _containers.Where(c =>
                c.ContainerCategory == ContainerCategory.SituationStorage ||
                c.ContainerCategory == ContainerCategory.Threshold ||
            c.ContainerCategory == ContainerCategory.Output))
            {
                aspects.CombineAspects(container.GetTotalAspects(includeElementAspects));
            }

            return aspects;

        }

        public SituationState Continue(IRecipeConductor rc, float interval, bool waitForGreedyAnim = false)
        {
            switch (State)
            {
                case SituationState.ReadyToReset:

                    Reset();
                    break;

                case SituationState.Unstarted:

                    break;

                case SituationState.ReadyToStart:

                    Begin();
                    break;


                case SituationState.Ongoing:

                    // Execute if we've got no time remaining and we're not waiting for a greedy anim
                    // UNLESS timer has gone negative for 5 seconds. In that case sometime is stuck and we need to break out
                    if (TimeRemaining <= 0 && (!waitForGreedyAnim || TimeRemaining < -5.0f))
                    {
                        RequireExecution(rc);
                    }
                    else
                    {
                        TimeRemaining = TimeRemaining - interval;
                        Ongoing();
                    }

                    break;

                case SituationState.RequiringExecution:
            
                End(rc);
                  break;

                case SituationState.Complete:

                  break;

                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return State;
        }



        public void Begin()
        {
            State = SituationState.Ongoing;

            SituationEventData d = SituationEventData.Create(this);


            var storageContainer = GetSingleContainerByCategory(ContainerCategory.SituationStorage);
            storageContainer.AcceptStacks(GetStacks(ContainerCategory.Threshold),
                new Context(Context.ActionSource.SituationStoreStacks));

            var prediction = GetRecipePrediction();
            PossiblySignalImpendingDoom(prediction.SignalEndingFlavour);

            foreach (var subscriber in subscribers)
            {
                subscriber.SituationBeginning(d);
                subscriber.RecipePredicted(prediction);
            }

        }





        private void PossiblySignalImpendingDoom(EndingFlavour endingFlavour)
        {
            var tabletopManager = Registry.Get<TabletopManager>();
            if (endingFlavour != EndingFlavour.None)
                tabletopManager.SignalImpendingDoom(_anchor);
            else
                tabletopManager.NoMoreImpendingDoom(_anchor);

        }

        private void Ongoing()
        {

            SituationEventData d = SituationEventData.Create(this);

            State = SituationState.Ongoing;
            foreach (var subscriber in subscribers)
                subscriber.SituationOngoing(d);
        }

        private void RequireExecution(IRecipeConductor rc)
        {
            State = SituationState.RequiringExecution;

            IList<RecipeExecutionCommand> recipeExecutionCommands = rc.GetActualRecipesToExecute(currentPrimaryRecipe);

            //actually replace the current recipe with the first on the list: any others will be additionals,
            //but we want to loop from this one.
            if (recipeExecutionCommands.First().Recipe.Id != currentPrimaryRecipe.Id)
                currentPrimaryRecipe = recipeExecutionCommands.First().Recipe;


            foreach (var container in _containers)
                container
                    .ActivatePreRecipeExecutionBehaviour(); //currently, this is just marking items in slots for consumption, so it happens once for everything. I might later want to run it per effect command, tho
            //on the other hand, I *do* want this to run before the stacks are moved to storage.
            //but finally, it would probably do better in AcceptStack on the recipeslot anyway

            //move any elements currently in OngoingSlots to situation storage
            //NB we're doing this *before* we execute the command - the command may affect these elements too
            GetSingleContainerByCategory(ContainerCategory.SituationStorage).AcceptStacks(
                GetStacks(ContainerCategory.Threshold), new Context(Context.ActionSource.SituationStoreStacks));


            foreach (var c in recipeExecutionCommands)
            {
                SituationEffectCommand ec = new SituationEffectCommand(c.Recipe,
                    c.Recipe.ActionId != currentPrimaryRecipe.ActionId, c.Expulsion);
                if (ec.AsNewSituation)
                    CreateNewSituation(ec);
                else
                {
                    Registry.Get<Character>().AddExecutionsToHistory(ec.Recipe.Id, 1); //can we make 
                    var executor = new SituationEffectExecutor(Registry.Get<TabletopManager>());
                    executor.RunEffects(ec, GetSingleContainerByCategory(ContainerCategory.SituationStorage), Registry.Get<Character>(), Registry.Get<IDice>());

                    if (!string.IsNullOrEmpty(ec.Recipe.Ending))
                    {
                        var ending = Registry.Get<ICompendium>().GetEntityById<Ending>(ec.Recipe.Ending);
                        Registry.Get<TabletopManager>() .EndGame(ending, this._anchor); //again, ttm (or successor) is subscribed. We should do it through there.
                    }
                    
                    
                    TryOverrideVerbIcon(GetAspectsAvailableToSituation(true));


                }

                foreach (var subscriber in subscribers)
                {
                    SituationEventData d = SituationEventData.Create(this);
                    d.EffectCommand = ec;
                    subscriber.SituationExecutingRecipe(d);
                }

            }
        }
        //TODO: I don't love this - it goes outside the subscription flow - but there's an expensive lookup here. I should cache it and then rationalise.
        private void TryOverrideVerbIcon(IAspectsDictionary forAspects)
        {
            //if we have an element in the situation now that overrides the verb icon, update it
            string overrideIcon = Registry.Get<ICompendium>().GetVerbIconOverrideFromAspects(forAspects);
            if (!string.IsNullOrEmpty(overrideIcon))
            {
                _anchor.DisplayOverrideIcon(overrideIcon);
                _window.DisplayIcon(overrideIcon);
            }
        }
        private void CreateNewSituation(SituationEffectCommand effectCommand)
        {
            List<ElementStackToken> stacksToAddToNewSituation = new List<ElementStackToken>();
            //if there's an expulsion
            if (effectCommand.Expulsion.Limit > 0)
            {
                //find one or more matching stacks. Important! the limit applies to stacks, not cards. This might need to change.
                AspectMatchFilter filter = new AspectMatchFilter(effectCommand.Expulsion.Filter);
                var filteredStacks = filter.FilterElementStacks(GetStacks(ContainerCategory.SituationStorage)).ToList();

                if (filteredStacks.Any() && effectCommand.Expulsion.Limit > 0)
                {
                    while (filteredStacks.Count > effectCommand.Expulsion.Limit)
                    {
                        filteredStacks.RemoveAt(filteredStacks.Count - 1);
                    }

                    stacksToAddToNewSituation = filteredStacks;
                }

            }

            IVerb verbForNewSituation = Registry.Get<ICompendium>().GetEntityById<BasicVerb>(effectCommand.Recipe.ActionId);

            if (verbForNewSituation == null)
                verbForNewSituation = new CreatedVerb(effectCommand.Recipe.ActionId, effectCommand.Recipe.Label, effectCommand.Recipe.Description);


            var scc = new SituationCreationCommand(verbForNewSituation, effectCommand.Recipe, SituationState.ReadyToStart, _anchor.Location, _anchor);
            Registry.Get<TabletopManager>().BeginNewSituation(scc, stacksToAddToNewSituation); //tabletop manager is a subscriber, right? can we run this (or access to its successor) through that flow?

        }

    private void End(IRecipeConductor rc) {
			

			var linkedRecipe = rc.GetLinkedRecipe(currentPrimaryRecipe);
			
			if (linkedRecipe!=null) {
				//send the completion description before we move on
				INotification notification = new Notification(currentPrimaryRecipe.Label, currentPrimaryRecipe.Description);
                SendNotificationToSubscribers(notification);
    
                //I think this code duplicates ActivateRecipe, below
				currentPrimaryRecipe = linkedRecipe;
				TimeRemaining = currentPrimaryRecipe.Warmup;
				if(TimeRemaining>0) //don't play a sound if we loop through multiple linked ones
				{
					if (currentPrimaryRecipe.SignalImportantLoop)
						SoundManager.PlaySfx("SituationLoopImportant");
					else
						SoundManager.PlaySfx("SituationLoop");

				}
				Begin();
			}
			else { 
				Complete();
			}
    }

    public void SendNotificationToSubscribers(INotification notification)
    {
        //Check for possible text refinements based on the aspects in context
        var aspectsInSituation = GetAspectsAvailableToSituation(true);


        TextRefiner tr = new TextRefiner(aspectsInSituation);


        Notification refinedNotification = new Notification(notification.Title,
            tr.RefineString(notification.Description));
            

            var d = SituationEventData.Create(this);
        d.Notification = refinedNotification;

            foreach (var subscriber in subscribers)
                subscriber.ReceiveNotification(d);


         
        }

    private void Complete() {
        State = SituationState.Complete;


        var outputStacks = GetStacks(ContainerCategory.SituationStorage);
        AcceptStacks(ContainerCategory.Output, outputStacks, new Context(Context.ActionSource.SituationResults));
        
        AttemptAspectInductions(currentPrimaryRecipe,outputStacks);

            
        if (currentPrimaryRecipe.PortalEffect != PortalEffect.None)
        {
            // Once again, very much subscriber territory

                Registry.Get<TabletopManager>().ShowMansusMap(this, _anchor.transform,
                    currentPrimaryRecipe.PortalEffect);
        }

            foreach (var subscriber in subscribers)
            {
                var d = SituationEventData.Create(this);
                subscriber.SituationComplete(d);
            }


            SoundManager.PlaySfx("SituationComplete"); //this could run through that Echo obj
    }


    private void AttemptAspectInductions(Recipe currentRecipe, List<ElementStackToken> outputStacks) // this should absolutely go through subscription - something to succeed ttm
    {
        //If any elements in the output, or in the situation itself, have inductions, test whether to start a new recipe

   var inducingAspects = new AspectsDictionary();

        //shrouded cards don't trigger inductions. This is because we don't generally want to trigger an induction
        //for something that has JUST BEEN CREATED. This started out as a hack, but now we've moved from 'face-down'
        //to 'shrouded' it feels more suitable.

        foreach (var os in outputStacks)
        {
            if (!os.Shrouded())
                inducingAspects.CombineAspects(os.GetAspects());
        }


        inducingAspects.CombineAspects(currentRecipe.Aspects);


        foreach (var a in inducingAspects)
        {
            var aspectElement = Registry.Get<ICompendium>().GetEntityById<Element>(a.Key);

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
                CreateRecipeFromInduction(Registry.Get<ICompendium>() .GetEntityById<Recipe>(induction.Id), aspectElement.Id);
        }
    }

    void CreateRecipeFromInduction(Recipe inducedRecipe, string aspectID) // yeah this *definitely* should be through subscription!
    {
        if (inducedRecipe == null)
        {
            NoonUtility.Log("unknown recipe " + inducedRecipe + " in induction for " + aspectID);
            return;
        }

        var inductionRecipeVerb = new CreatedVerb(inducedRecipe.ActionId,
            inducedRecipe.Label, inducedRecipe.Description);
        SituationCreationCommand inducedSituation = new SituationCreationCommand(inductionRecipeVerb,
            inducedRecipe, SituationState.ReadyToStart, _anchor.Location, _anchor);
        Registry.Get<TabletopManager>().BeginNewSituation(inducedSituation, new List<ElementStackToken>());
    }

        public void Close()
    {
        DumpThresholdStacks();
        _window.Hide();
        _anchor.DisplayAsClosed();
    }

    public bool IsOpen()
    {
        return _window.IsOpen;
    }

    public void OpenAt(TokenLocation location)
    {
        _anchor.DisplayAsOpen();
        _window.Show(location.Position);
            
        Registry.Get<TabletopManager>().CloseAllSituationWindowsExcept(_anchor.EntityId);
    }

    public void OpenAtCurrentLocation()
    {
        var currentLocation = GetAnchorLocation();
        if(currentLocation==null)
            OpenAt(new TokenLocation(Vector3.zero));
        else
            OpenAt(currentLocation);
    }



    public TokenContainer GetFirstAvailableThresholdForStackPush(ElementStackToken stack)
    {
        var thresholds = GetContainersByCategory(ContainerCategory.Threshold);
        foreach (var t in thresholds)

            if (!t.IsGreedy && t.GetMatchForStack(stack).MatchType ==SlotMatchForAspectsType.Okay)
                return t;

        return Registry.Get<NullContainer>();
    }


        public bool PushDraggedStackIntoThreshold(ElementStackToken stack)
        {
            var thresholdContainer = GetFirstAvailableThresholdForStackPush(stack);
            var possibleIncumbent = thresholdContainer.GetStacks().FirstOrDefault();
            if (possibleIncumbent != null)
            {
                possibleIncumbent.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
            }

            return  thresholdContainer.TryAcceptStackAsThreshold(stack);

        }

        private void ContainerStacksChanged(ContainerStacksChangedArgs _stacksChangedArgs)
        {
            var aspectsInContext =
                Registry.Get<TabletopManager>().GetAspectsInContext(GetAspectsAvailableToSituation(true));

        currentPredictedRecipe = Registry.Get<ICompendium>()
                .GetPredictedRecipe(currentPrimaryRecipe, aspectsInContext, Verb, Registry.Get<Character>());

            //we now have the recipe that would be available the next time an execution point occurs.
            //we *don't* want to change the current recipe based on this - but we do want to update the prediction for subscribers
            
            //so I *think* it's useful to store the predicted recipe? this also allows for caching

            SituationEventData data = SituationEventData.Create(this);
            
            foreach (var s in subscribers)
                s.ContainerContentsUpdated(data);

        }

        public void DumpThresholdStacks()
        {
            var slotted = GetStacks(ContainerCategory.Threshold);
            foreach (var item in slotted)
                item.ReturnToTabletop(new Context(Context.ActionSource.PlayerDumpAll));

            Reset();

 
        }

        public void CollectOutputStacks()
        {
            var results = GetStacks(ContainerCategory.Output);
            foreach (var item in results)
                item.ReturnToTabletop(new Context(Context.ActionSource.PlayerDumpAll));
            
            Reset();

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
            var stacksToDecay = GetStacks(ContainerCategory.Output);
            foreach (var s in stacksToDecay)
                s.Decay(interval);
        }


        private RecipePrediction GetRecipePrediction()
        {
            TabletopManager ttm = Registry.Get<TabletopManager>();
            var aspectsInSituation = GetAspectsAvailableToSituation(true);

            var context = ttm.GetAspectsInContext(aspectsInSituation);
            RecipeConductor rc = new RecipeConductor(Registry.Get<ICompendium>(),
                context, Registry.Get<IDice>(),
                Registry.Get<Character>());
            var prediction = rc.GetPredictionForFollowupRecipe(currentPrimaryRecipe);

            TextRefiner tr = new TextRefiner(aspectsInSituation);
            prediction.DescriptiveText = tr.RefineString(prediction.DescriptiveText);
            return prediction;

        }


        public void ActivateRecipe()
        {
            
            if (State != SituationState.Unstarted)
                return;

            var aspects = GetAspectsAvailableToSituation(true);
            var tabletopManager = Registry.Get<TabletopManager>();
            var aspectsInContext = tabletopManager.GetAspectsInContext(aspects);


            var recipe = Registry.Get<ICompendium>().GetFirstMatchingRecipe(aspectsInContext, Verb.Id, Registry.Get<Character>(), false);

            //no recipe found? get outta here
            if (recipe == null)
                return;

            currentPrimaryRecipe = recipe;
            TimeRemaining = currentPrimaryRecipe.Warmup;
            State = SituationState.ReadyToStart;

            SoundManager.PlaySfx("SituationBegin");

            //called here in case starting slots trigger consumption
            foreach(var t in GetContainersByCategory(ContainerCategory.Threshold))
                t.ActivatePreRecipeExecutionBehaviour();

            //now move the stacks out of the starting slots into storage
            AcceptStacks(ContainerCategory.SituationStorage,GetStacks(ContainerCategory.Threshold));

            //The game might be paused! or the player might just be incredibly quick off the mark
            //so immediately continue with a 0 interval - this won't advance time, but will update the visuals in the situation window
            //(which among other things should make the starting slot unavailable

            RecipeConductor rc = new RecipeConductor(Registry.Get<ICompendium>(),
                aspectsInContext, Registry.Get<IDice>(), Registry.Get<Character>()); //reusing the aspectsInContext from above

            Continue(rc, 0);

   
        }

 
    }

}
