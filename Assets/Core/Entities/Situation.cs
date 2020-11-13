using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Enums;
using Assets.Core.Fucine;
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

    public class Situation: ISphereEventSubscriber
    {
        public SituationState State { get; set; }
        public Recipe currentPrimaryRecipe { get; set; }
        public RecipePrediction CurrentRecipePrediction{ get; set; }

        public float TimeRemaining { private set; get; }

        public float intervalForLastHeartbeat { private set; get; }

        public float Warmup
        {
            get { return currentPrimaryRecipe.Warmup; }
        }

        public string RecipeId
        {
            get { return currentPrimaryRecipe == null ? null : currentPrimaryRecipe.Id; }
        }

        public readonly IVerb Verb;
        public readonly Species Species;
        private readonly List<ISituationSubscriber> subscribers = new List<ISituationSubscriber>();
        private readonly HashSet<Sphere> _spheres = new HashSet<Sphere>();
        public string OverrideTitle { get; set; }

        private Token _anchor;
        private SituationWindow _window;
        private bool greedyAnimIsActive;
        public SituationPath Path { get; }
        public bool IsOpen { get; private set; }
        public RecipeBeginningEffectCommand CurrentBeginningEffectCommand;
        public RecipeCompletionEffectCommand currentCompletionEffectCommand;


        public TokenLocation GetAnchorLocation()
        {
            return _anchor.Location;
        }

        public Vector3 GetWindowLocation()
        {
            return _window.positioner.GetPosition();
        }

        public const float HOUSEKEEPING_CYCLE_BEATS = 1f;


        public Situation(SituationCreationCommand command)
        {
            Verb = command.GetBasicOrCreatedVerb();
            Species = command.Species;
            TimeRemaining = command.TimeRemaining ?? 0;
            State = command.State;
            currentPrimaryRecipe = command.Recipe;
            OverrideTitle = command.OverrideTitle;
            Path = command.SituationPath;
        }



        public void AttachAnchor(Token newAnchor)
        {
            _anchor = newAnchor;
            AddSubscriber(_anchor);
            _anchor.Populate(this);
        }



        public void AttachWindow(SituationWindow newWindow,SituationCreationCommand command)
        {
            _window = newWindow;
            AddSubscriber(_window);


            _window.OnWindowClosed.AddListener(Close);
            _window.OnStart.AddListener(ActivateRecipe);
            _window.OnCollect.AddListener(CollectOutputStacks);
            _window.OnContainerAdded.AddListener(AddContainer);
            _window.OnContainerRemoved.AddListener(RemoveContainer);

            _window.Populate(command);

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
            if (currentPrimaryRecipe.Slots.Any())
                return currentPrimaryRecipe.Slots;
            else
                return new List<SlotSpecification>();
        }


        private void Reset()
        {
            currentPrimaryRecipe = NullRecipe.Create(Verb);
            CurrentRecipePrediction = GetUpdatedRecipePrediction();
            TimeRemaining = 0;
            State = SituationState.Unstarted;
        }

        public void Halt()
        {
            if (State != SituationState.Complete && State != SituationState.ReadyToReset &&
                State != SituationState.Unstarted
            ) //don't halt if the situation is not running. This is not only superfluous but dangerous: 'complete' called from an already completed verb has bad effects
                Complete();

            //If we leave anything in the ongoing slot, it's lost, and also the situation ends up in an anomalous state which breaks loads
            AcceptStacks(SphereCategory.SituationStorage, GetStacks(SphereCategory.Threshold));

        }


        public IEnumerable<Sphere> GetSpheresByCategory(SphereCategory category)
        {
            return _spheres.Where(c => c.SphereCategory == category);
        }

        private Sphere GetSingleSphereByCategory(SphereCategory category)
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
            greedyAnimIsActive = true;
            slotAnim.onElementSlotAnimDone += HandleOnGreedySlotAnimDone;

            TabletopManager.RequestNonSaveableState(TabletopManager.NonSaveableType.Greedy, true);

        }

        void HandleOnGreedySlotAnimDone(ElementStack element, TokenLocation destination,
            Sphere destinatinoSlot)
        {
            greedyAnimIsActive = false;
            TabletopManager.RequestNonSaveableState(TabletopManager.NonSaveableType.Greedy, false);
        }


        public HeartbeatResponse ExecuteHeartbeat(float interval)
        {

        

            Continue(interval, greedyAnimIsActive);

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
                    _spheres.Where(c =>
                        c.SphereCategory == SphereCategory.Threshold && c.IsGreedy &&
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

        
    public void AcceptStack(SphereCategory forSphereCategory, ElementStack stack, Context context)
        {
            var stackTokenList = new List<ElementStack> {stack};
            AcceptStacks(forSphereCategory, stackTokenList, context);
        }

        public void AcceptStacks(SphereCategory forSphereCategory, IEnumerable<ElementStack> stacks,
            Context context)
        {
            var acceptingContainer = GetSingleSphereByCategory(forSphereCategory);
            acceptingContainer.AcceptStacks(stacks, context);
        }

        public void AcceptStacks(SphereCategory forSphereCategory, IEnumerable<ElementStack> stacks)
        {
            AcceptStacks(forSphereCategory,stacks,new Context(Context.ActionSource.Unknown));
        }

        public List<ElementStack> GetAllStacksInSituation()
        {
            List<ElementStack> stacks = new List<ElementStack>();
            foreach (var container in _spheres)
                stacks.AddRange(container.GetStackTokens());
            return stacks;
        }


        public List<ElementStack> GetStacks(SphereCategory forSphereCategory)
        {
            List<ElementStack> stacks = new List<ElementStack>();
            foreach (var container in _spheres.Where(c => c.SphereCategory == forSphereCategory))
                stacks.AddRange(container.GetStackTokens());

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

        public SituationState Continue(float interval, bool waitForGreedyAnim = false)
        {
            intervalForLastHeartbeat = interval;

            switch (State)
            {
                case SituationState.ReadyToReset:
                    Reset();
                    break;

                case SituationState.Unstarted:
                    break;

                case SituationState.ReadyToStart:
                    State = SituationState.Ongoing;
                    CurrentRecipePrediction = GetUpdatedRecipePrediction();
                    CurrentBeginningEffectCommand = new RecipeBeginningEffectCommand(currentPrimaryRecipe.Slots, CurrentRecipePrediction.BurnImage);
                    var storageContainer = GetSingleSphereByCategory(SphereCategory.SituationStorage);
                    storageContainer.AcceptStacks(GetStacks(SphereCategory.Threshold),
                        new Context(Context.ActionSource.SituationStoreStacks));
                    break;


                case SituationState.ReadyToContinue: //special case: we want to re-initiate an ongoing state rather than start a new one, perhaps because we reloaded. So we won't, for instance, move ongoing slots contents into storage.
                    State = SituationState.Ongoing;
                    CurrentBeginningEffectCommand = new RecipeBeginningEffectCommand(currentPrimaryRecipe.Slots, CurrentRecipePrediction.BurnImage);
                    break;

                case SituationState.Ongoing:
                    // Execute if we've got no time remaining and we're not waiting for a greedy anim
                    // UNLESS timer has gone negative for 5 seconds. In that case sometime is stuck and we need to break out
                    if (TimeRemaining <= 0 && (!waitForGreedyAnim || TimeRemaining < -5.0f))
                        RequireExecution();
                    else
                        TimeRemaining = TimeRemaining - interval;
                    

                    break;

                case SituationState.RequiringExecution:
                    var tc = Registry.Get<SphereCatalogue>();
                    var aspectsInContext = tc.GetAspectsInContext(GetAspectsAvailableToSituation(true));

                    var rc=new RecipeConductor(aspectsInContext, Registry.Get<Character>());

                    var linkedRecipe = rc.GetLinkedRecipe(currentPrimaryRecipe);

                    if (linkedRecipe != null)
                    {
                        //send the completion description before we move on
                        INotification notification = new Notification(currentPrimaryRecipe.Label, currentPrimaryRecipe.Description);
                        SendNotificationToSubscribers(notification);

                        //I think this code duplicates ActivateRecipe, below
                        currentPrimaryRecipe = linkedRecipe;
                        TimeRemaining = currentPrimaryRecipe.Warmup;
                        if (TimeRemaining > 0) //don't play a sound if we loop through multiple linked ones
                        {
                            if (currentPrimaryRecipe.SignalImportantLoop)
                                SoundManager.PlaySfx("SituationLoopImportant");
                            else
                                SoundManager.PlaySfx("SituationLoop");

                        }

                        State = SituationState.ReadyToStart;
                    }
                    else
                    {
                        Complete();
                    }
                    break;

                case SituationState.Complete:
                    break;

                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var subscriber in subscribers)
            {
                subscriber.DisplaySituationState(this);
            }

            CurrentBeginningEffectCommand = null; ;
            currentCompletionEffectCommand = null;

            return State;
        }





        private void PossiblySignalImpendingDoom(EndingFlavour endingFlavour)
        {
            var tabletopManager = Registry.Get<TabletopManager>();
            if (endingFlavour != EndingFlavour.None)
                tabletopManager.SignalImpendingDoom(_anchor);
            else
                tabletopManager.NoMoreImpendingDoom(_anchor);

        }

        private void RequireExecution()
        {
            State = SituationState.RequiringExecution;
            var tc = Registry.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(GetAspectsAvailableToSituation(true));

            RecipeConductor rc =new RecipeConductor(aspectsInContext, Registry.Get<Character>());

            IList<RecipeExecutionCommand> recipeExecutionCommands = rc.GetRecipeExecutionCommands(currentPrimaryRecipe);

            //actually replace the current recipe with the first on the list: any others will be additionals,
            //but we want to loop from this one.
            if (recipeExecutionCommands.First().Recipe.Id != currentPrimaryRecipe.Id)
                currentPrimaryRecipe = recipeExecutionCommands.First().Recipe;


            foreach (var container in _spheres)
                container
                    .ActivatePreRecipeExecutionBehaviour(); //currently, this is just marking items in slots for consumption, so it happens once for everything. I might later want to run it per effect command, tho
            //on the other hand, I *do* want this to run before the stacks are moved to storage.
            //but finally, it would probably do better in AcceptStack on the recipeslot anyway

            //move any elements currently in OngoingSlots to situation storage
            //NB we're doing this *before* we execute the command - the command may affect these elements too
            GetSingleSphereByCategory(SphereCategory.SituationStorage).AcceptStacks(
                GetStacks(SphereCategory.Threshold), new Context(Context.ActionSource.SituationStoreStacks));


            foreach (var c in recipeExecutionCommands)
            {
                RecipeCompletionEffectCommand currentEffectCommand = new RecipeCompletionEffectCommand(c.Recipe,
                    c.Recipe.ActionId != currentPrimaryRecipe.ActionId, c.Expulsion,c.ToPath);
                if (currentEffectCommand.AsNewSituation)
                    CreateNewSituation(currentEffectCommand);
                else
                {
                    Registry.Get<Character>().AddExecutionsToHistory(currentEffectCommand.Recipe.Id, 1); //can we make 
                    var executor = new SituationEffectExecutor(Registry.Get<TabletopManager>());
                    executor.RunEffects(currentEffectCommand, GetSingleSphereByCategory(SphereCategory.SituationStorage), Registry.Get<Character>(), Registry.Get<IDice>());

                    if (!string.IsNullOrEmpty(currentEffectCommand.Recipe.Ending))
                    {
                        var ending = Registry.Get<ICompendium>().GetEntityById<Ending>(currentEffectCommand.Recipe.Ending);
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
            string overrideIcon = Registry.Get<ICompendium>().GetVerbIconOverrideFromAspects(forAspects);
            if (!string.IsNullOrEmpty(overrideIcon))
            {
                _anchor.DisplayOverrideIcon(overrideIcon);
                _window.DisplayIcon(overrideIcon);
            }
        }
        private void CreateNewSituation(RecipeCompletionEffectCommand effectCommand)
        {
            List<ElementStack> stacksToAddToNewSituation = new List<ElementStack>();
            //if there's an expulsion
            if (effectCommand.Expulsion.Limit > 0)
            {
                //find one or more matching stacks. Important! the limit applies to stacks, not cards. This might need to change.
                AspectMatchFilter filter = new AspectMatchFilter(effectCommand.Expulsion.Filter);
                var filteredStacks = filter.FilterElementStacks(GetStacks(SphereCategory.SituationStorage)).ToList();

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

            TokenLocation newAnchorLocation;

            if (effectCommand.ToPath != SpherePath.Current())
                newAnchorLocation = new TokenLocation(Vector3.zero, effectCommand.ToPath);
            else
                newAnchorLocation = _anchor.Location;


            var scc = new SituationCreationCommand(verbForNewSituation, effectCommand.Recipe,
                SituationState.ReadyToStart, newAnchorLocation, _anchor);
            Registry.Get<SituationsCatalogue>()
                .BeginNewSituation(scc,
                    stacksToAddToNewSituation); //tabletop manager is a subscriber, right? can we run this (or access to its successor) through that flow?

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

    private void Complete() {
        State = SituationState.Complete;


        var outputStacks = GetStacks(SphereCategory.SituationStorage);
        AcceptStacks(SphereCategory.Output, outputStacks, new Context(Context.ActionSource.SituationResults));
        
        AttemptAspectInductions(currentPrimaryRecipe,outputStacks);


            SoundManager.PlaySfx("SituationComplete"); //this could run through that Echo obj
    }


    private void AttemptAspectInductions(Recipe currentRecipe, List<ElementStack> outputStacks) // this should absolutely go through subscription - something to succeed ttm
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
        Registry.Get<SituationsCatalogue>().BeginNewSituation(inducedSituation, new List<ElementStack>());
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
            
        Registry.Get<TabletopManager>().CloseAllSituationWindowsExcept(_anchor.EntityId);
    }

    public void OpenAtCurrentLocation()
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

            if (!t.IsGreedy && t.GetMatchForStack(stack).MatchType ==SlotMatchForAspectsType.Okay)
                return t;

        return Registry.Get<NullContainer>();
    }


        public bool PushDraggedStackIntoThreshold(ElementStack stack)
        {
            var thresholdContainer = GetFirstAvailableThresholdForStackPush(stack);
            var possibleIncumbent = thresholdContainer.GetStackTokens().FirstOrDefault();
            if (possibleIncumbent != null)
            {
                possibleIncumbent.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
            }

            return  thresholdContainer.TryAcceptStackAsThreshold(stack);

        }



        public void DumpUnstartedBusiness()
        {
       //deferring this a bit longer. I need to think about how to connect startingslot behaviour with the BOH model:
       //slot behaviour: dump when window closed?
       //slot behaviour: block for certain kinds of interaction? using existing block?
       //slot behaviour: specify connection type with other containers? ie expand 'greedy' effect to mean multiple things and directions
            //if(State!=SituationState.Ongoing)
            //{
            //    var slotted = GetStacks(ContainerCategory.Threshold);
            //    foreach (var item in slotted)
            //        item.ReturnToTabletop(new Context(Context.ActionSource.PlayerDumpAll));
            //}


        }

        public void CollectOutputStacks()
        {
            var results = GetStacks(SphereCategory.Output);
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
            var stacksToDecay = GetStacks(SphereCategory.Output);
            foreach (var s in stacksToDecay)
                s.Decay(interval);
        }


        public void ActivateRecipe()
        {
            
            if (State != SituationState.Unstarted)
                return;

            var aspects = GetAspectsAvailableToSituation(true);
            var tc = Registry.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(aspects);


            var recipe = Registry.Get<ICompendium>().GetFirstMatchingRecipe(aspectsInContext, Verb.Id, Registry.Get<Character>(), false);

            //no recipe found? get outta here
            if (recipe == null)
                return;

            currentPrimaryRecipe = recipe;
            TimeRemaining = currentPrimaryRecipe.Warmup;
            State = SituationState.ReadyToStart;

            SoundManager.PlaySfx("SituationBegin");

            //called here in case starting slots trigger consumption
            foreach(var t in GetSpheresByCategory(SphereCategory.Threshold))
                t.ActivatePreRecipeExecutionBehaviour();

            //now move the stacks out of the starting slots into storage
            AcceptStacks(SphereCategory.SituationStorage,GetStacks(SphereCategory.Threshold));

            //The game might be paused! or the player might just be incredibly quick off the mark
            //so immediately continue with a 0 interval - this won't advance time, but will update the visuals in the situation window
            //(which among other things should make the starting slot unavailable

            RecipeConductor rc = new RecipeConductor(aspectsInContext, Registry.Get<Character>()); //reusing the aspectsInContext from above

            Continue(0);

   
        }


        private RecipePrediction GetUpdatedRecipePrediction()
        {
            var aspectsAvailableToSituation = GetAspectsAvailableToSituation(true);

            var aspectsInContext =
                Registry.Get<SphereCatalogue>().GetAspectsInContext(aspectsAvailableToSituation);

            RecipeConductor rc = new RecipeConductor(aspectsInContext,Registry.Get<Character>());

            return rc.GetPredictionForFollowupRecipe(currentPrimaryRecipe, State, Verb);
        }

        public void NotifyTokensChangedForContainer(TokenEventArgs args)
        {

            CurrentRecipePrediction = GetUpdatedRecipePrediction();
            PossiblySignalImpendingDoom(CurrentRecipePrediction.SignalEndingFlavour);

            foreach (var s in subscribers)
                s.ContainerContentsUpdated(this);
        }

        public void OnTokenClicked(TokenEventArgs args)
        {
            }

        public void OnTokenReceivedADrop(TokenEventArgs args)
        {
            }

        public void OnTokenPointerEntered(TokenEventArgs args)
        {
            }

        public void OnTokenPointerExited(TokenEventArgs args)
        {
            }

        public void OnTokenDoubleClicked(TokenEventArgs args)
        {
            }

        public void OnTokenDragged(TokenEventArgs args)
        {
            //
        }
    }

}
