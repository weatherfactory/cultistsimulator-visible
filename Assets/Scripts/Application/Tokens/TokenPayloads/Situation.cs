using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Commands;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.States;
using SecretHistories.UI;
using Assets.Logic;
using Assets.Scripts.Application.Entities;
using Assets.Scripts.Application.Entities.NullEntities;
using Assets.Scripts.Application.Fucine;
using Assets.Scripts.Application.Infrastructure.Events;
using Assets.Scripts.Application.Spheres;
using Newtonsoft.Json;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;
using SecretHistories.Abstract;
using SecretHistories.Constants;
using SecretHistories.Core;
using SecretHistories.Logic;
using SecretHistories.Manifestations;
using SecretHistories.States.TokenStates;
using UnityEngine;
using UnityEngine.UIElements;

namespace SecretHistories.Entities {

    [IsEncaustableClass(typeof(SituationCreationCommand))]
    public class Situation: ISphereEventSubscriber,ITokenPayload
    {
#pragma warning disable 67
        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
#pragma warning restore 67

        [Encaust]
        public StateEnum StateIdentifier => State.Identifier;


        [Encaust] public string RecipeId => Recipe.Id;

        [DontEncaust]
        public Recipe Recipe { get; set; }

        [Encaust]
        public float TimeRemaining => _timeshadow.LifetimeRemaining;

        [Encaust]
        public string VerbId => Verb.Id;

        [DontEncaust]
        public Verb Verb { get; set; }

        [Encaust]
        public string OverrideTitle { get; set; }


        [Encaust] public string Id { get; protected set; }
        public FucinePath GetAbsolutePath()
        {
            if(_token==null)
                return new NullFucinePath();
            var pathAbove = _token.Sphere.GetAbsolutePath();
            var absolutePath = pathAbove.AppendToken(this.Id);
            return absolutePath;
        }

        public RectTransform GetRectTransform()
        {
            return Token.TokenRectTransform;
        }

        [DontEncaust] public FucinePath AbsolutePath => null;

        [DontEncaust] public string EntityId => Verb.Id;

        [Encaust]
        public List<IDominion> Dominions => new List<IDominion>(_registeredDominions);

        [Encaust]
        public bool IsOpen { get; private set; }

        [Encaust]
        public SituationCommandQueue CommandQueue { get; set; } = new SituationCommandQueue();

      //  [Encaust]
      //  public RecipeCompletionEffectCommand CurrentCompletionEffectCommand { get; set; } = new RecipeCompletionEffectCommand();

        [Encaust] public Dictionary<string, int> Mutations => new Dictionary<string, int>();
        [Encaust] public int Quantity => 1;

        [DontEncaust] public RecipePrediction CurrentRecipePrediction => _currentRecipePrediction;
        [DontEncaust] public SituationState State { get; set; }
        [DontEncaust] public float Warmup => Recipe.Warmup;

        
        [DontEncaust] public string Label => GetMostRecentNoteLabel();
        [DontEncaust] public string Description => GetMostRecentNoteDescription();
        [DontEncaust] public float IntervalForLastHeartbeat => _timeshadow.LastInterval;
        [DontEncaust]
        public string UniquenessGroup
        {
            get
            {
                if (Unique)
                    return Verb.Id;
                return string.Empty;
            }
        }

        [DontEncaust]
        public bool Unique
        {
            get
            {
                if (!Verb.Spontaneous)
                    return true;
                if (!State.AllowDuplicateVerbIfVerbSpontaneous)
                    return true;

                return false;
            }
        }
        [DontEncaust]
        public string Icon
        {
            get
            {
                if (!string.IsNullOrEmpty(Verb.Icon))
                    return Verb.Icon;

                return Verb.Id;
            }

        }
        [DontEncaust]
        public Token Token
        {
            get
            {
                {
                    if (_token == null)
                        return NullToken.Create();
                    return _token;
                }
            }
        }




        private RecipePrediction _currentRecipePrediction;

        private readonly List<ISituationSubscriber> _subscribers = new List<ISituationSubscriber>();
        private readonly List<IDominion> _registeredDominions = new List<IDominion>();
        private readonly HashSet<Sphere> _spheres = new HashSet<Sphere>();
        
        private Timeshadow _timeshadow;
        private Token _token;


        public Situation(Verb verb,string id)
        {
            HornedAxe hornedAxe = Watchman.Get<HornedAxe>();

            hornedAxe.RegisterSituation(this);

            Id = id;
            Verb = verb;
            Recipe = NullRecipe.Create();
            _currentRecipePrediction = new RecipePrediction(Recipe, AspectsDictionary.Empty());

            State = new NullSituationState();
            _timeshadow = new Timeshadow(Recipe.Warmup,
                Recipe.Warmup,
                false);
        }




        public bool RegisterDominion(IDominion dominionToRegister)
        {
            dominionToRegister.OnSphereAdded.AddListener(AttachSphere);
            dominionToRegister.OnSphereRemoved.AddListener(DetachSphere);

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




        public void AttachSphere(Sphere sphere)
        {
            sphere.Subscribe(this);
            sphere.SetContainer(this);
            _spheres.Add(sphere);
        }

        public void DetachSphere(Sphere c)
        {
            c.Unsubscribe(this);
            _spheres.Remove(c);
        }

        public void UpdateCurrentRecipePrediction(RecipePrediction newRecipePrediction,Context context)
        {
            if (!newRecipePrediction.AddsMeaningfulInformation(_currentRecipePrediction))
                return;

            _currentRecipePrediction = newRecipePrediction;

              var addNoteCommand=new AddNoteCommand(newRecipePrediction.Title,newRecipePrediction.DescriptiveText,context);
                addNoteCommand.ExecuteOn(this);

        }




        public List<Sphere> GetSpheresActiveForCurrentState()
        {
            return new List<Sphere>(_spheres).Where(sphere => State.IsActiveInThisState(sphere) && !sphere.Defunct).ToList();
        }

        public List<Sphere> GetSpheresByCategory(SphereCategory category)
        {
            return new List<Sphere>(_spheres.Where(c => c.SphereCategory == category && !c.Defunct));
        }

        public Sphere GetSingleSphereByCategory(SphereCategory category)
        {
            try
            {
                return _spheres.SingleOrDefault(c => c.SphereCategory == category && !c.Defunct);
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

            return NullSphere.Create();

        }


        public Sphere GetSphereById(string sphereId)
        {
            return _spheres.SingleOrDefault(s => s.Id == sphereId && !s.Defunct);
        }

        public Type GetManifestationType(SphereCategory sphereCategory)
        {
            if (sphereCategory == SphereCategory.Meta)
                return typeof(NullManifestation);

            return typeof(VerbManifestation);
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            manifestation.InitialiseVisuals(this);
        }

        public virtual bool IsValid()
        {
            return true;
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public void FirstHeartbeat()
        {
             ExecuteHeartbeat(0f);
             NotifyStateChange();
             NotifyTimerChange();
        }

        public void ExecuteHeartbeat(float interval)
        {
            Continue(interval);
        }

        public bool CanInteractWith(ITokenPayload incomingTokenPayload)
        {
            if (GetAvailableThresholdsForStackPush(incomingTokenPayload).Count > 0)
                return true;

            return false;
        }

        public bool CanMergeWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public bool Retire(RetirementVFX vfx)
        {
            var spheresToRetire=new List<Sphere>(_spheres);
                foreach (var c in spheresToRetire)
                {
                    c.Retire(SphereRetirementType.Destructive);
                }

                TokenPayloadChangedArgs args = new TokenPayloadChangedArgs(this, PayloadChangeType.Retirement);
                args.VFX = RetirementVFX.VerbAnchorVanish;
                OnChanged?.Invoke(args);
                Watchman.Get<HornedAxe>().DeregisterSituation(this);

                return true;
        }

        public void InteractWithIncoming(Token token)
        {
            if (token.IsValidElementStack())
            {
                TryPushDraggedStackIntoThreshold(token);
                if (!IsOpen)
                    OpenAt(token.Location);
            }
            else
            {
                //something has gone awryy
                token.CurrentState= new RejectedByTokenState();
            }
        }

        public bool ReceiveNote(string label, string description,Context context)
        {
            //no infinite loops pls
            if (label == this.Label && description == this.Description)
                return true;

            if (!Situation.TextIntendedForDisplay(description))
                return true;

            var noteElementId = Watchman.Get<Compendium>().GetSingleEntity<Dictum>().NoteElementId;

            var existingNotesSpheres = GetSpheresByCategory(SphereCategory.Notes);

            
           var notesDominion = GetRelevantDominions(StateIdentifier,typeof(NotesSphere)).FirstOrDefault();
           if (notesDominion == null)
           {
               NoonUtility.Log($"No notes sphere and no notes dominion found: we won't add note {label}, then.");
                return false;
           }
           var notesSphereSpec=new SphereSpec(typeof(NotesSphere),$"{nameof(NotesSphere)}{existingNotesSpheres.Count}");
           var emptyNoteSphere = notesDominion.TryCreateSphere(notesSphereSpec);
  
            var newNoteCommand = new ElementStackCreationCommand(noteElementId, 1);
            newNoteCommand.Illuminations.Add(NoonConstants.TLG_NOTES_TITLE_KEY, label);
            newNoteCommand.Illuminations.Add(NoonConstants.TLG_NOTES_DESCRIPTION_KEY, description);

            var tokenCreationCommand =
                new TokenCreationCommand(newNoteCommand, TokenLocation.Default(emptyNoteSphere.GetAbsolutePath()));

            tokenCreationCommand.Execute(context,emptyNoteSphere);

            
            return true;
        }

        private Sphere LastSphereWithANote()
        {
            var notesSpheres = GetSpheresByCategory(SphereCategory.Notes);
            var lastSphereWithANote = notesSpheres.LastOrDefault(s => s.Tokens.Any());
            return lastSphereWithANote;
        }

        private string GetMostRecentNoteLabel()
        {
            var noteSphereToCheck = LastSphereWithANote();
            if (noteSphereToCheck == null)
                return string.Empty;

            var note = noteSphereToCheck.Tokens.First().Payload;
            string title = note.GetIllumination(NoonConstants.TLG_NOTES_TITLE_KEY);
            return title;


        }

        private string GetMostRecentNoteDescription()
        {
            var noteSphereToCheck = LastSphereWithANote();
            if (noteSphereToCheck == null)
                return string.Empty;

            var note = noteSphereToCheck.Tokens.First().Payload;
            string description = note.GetIllumination(NoonConstants.TLG_NOTES_DESCRIPTION_KEY);
            return description;
        }

        public void ShowNoMergeMessage(ITokenPayload incomingTokenPayload)
        {
            //
        }

        public void SetQuantity(int quantityToLeaveBehind, Context context)
        {
         //
        }

        public void ModifyQuantity(int unsatisfiedChange, Context context)
        {
            //
        }

        public void ExecuteTokenEffectCommand(IAffectsTokenCommand command)
        {
            command.ExecuteOn(this);
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

        public List<Token> GetElementTokensInSituation()
        {
            List<Token> stacks = new List<Token>();
            foreach (var sphere in _spheres)
                stacks.AddRange(sphere.GetElementTokens());
            return stacks;
        }

        public List<Token> GetTokens(SphereCategory forSphereCategory)
        {
            List<Token> stacks = new List<Token>();
            foreach (var container in _spheres.Where(c => c.SphereCategory == forSphereCategory && !c.Defunct))
                stacks.AddRange(container.Tokens);

            return stacks;
        }

        public List<Token> GetElementTokens(SphereCategory forSphereCategory)
        {
            List<Token> stacks = new List<Token>();
            foreach (var container in _spheres.Where(c => c.SphereCategory == forSphereCategory && !c.Defunct))
                stacks.AddRange(container.GetElementTokens());

            return stacks;
        }

        /// <summary>
        /// These are the aspects in the situation, not the aspects available to recipe criteria in the situation
        /// </summary>
        /// <param name="includeElementAspects"></param>
        /// <returns></returns>
        public AspectsDictionary GetAspects(bool includeElementAspects)
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

        

        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {
            throw new NotImplementedException("Does it mean anything to set mutations on a situation?");
        }

        public string GetSignature()
        {
            // Generate a distinctive signature

            return "situation_" + Verb.Id;
        }
        public Sphere GetEnRouteSphere()
        {
            if (Token.Sphere.GoverningSphereSpec.EnRouteSpherePath.IsValid())
                return Watchman.Get<HornedAxe>().GetSphereByPath(Token.Sphere.GoverningSphereSpec.EnRouteSpherePath);

            return Token.Sphere.GetContainer().GetEnRouteSphere();
        }

        public Sphere GetWindowsSphere()
        {
            if (Token.Sphere.GoverningSphereSpec.WindowsSpherePath.IsValid())
                return Watchman.Get<HornedAxe>().GetSphereByPath(Token.Sphere.GoverningSphereSpec.WindowsSpherePath);

            return Token.Sphere.GetContainer().GetWindowsSphere();
        }

        public void TransitionToState(SituationState newState)
        {


            State.Exit(this);
            newState.Enter(this);
            State = newState;
            NotifyStateChange();
            NotifyTimerChange();
        }

        private SituationState Continue(float interval)
        {
            CommandQueue.ExecuteCommandsFor(State.Identifier, this);
            _timeshadow.SpendTime(interval);

              State.Continue(this);

            return State;
        }


        public void NotifyStateChange()
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.SituationStateChanged(this);
                subscriber.TimerValuesChanged(this);
            }

            foreach (var dominion in _registeredDominions)
            {
                if (State.IsVisibleInThisState(dominion))
                    dominion.Evoke();
                else
                    dominion.Dismiss();
            }

            OnChanged?.Invoke(new TokenPayloadChangedArgs(this,PayloadChangeType.Update));
        }

        public void NotifyTimerChange()
        {
            
            foreach (var subscriber in _subscribers)
                subscriber.TimerValuesChanged(this);
            OnChanged?.Invoke(new TokenPayloadChangedArgs(this, PayloadChangeType.Update));
        }


        public void SendCommandToSubscribers(IAffectsTokenCommand command)
        {
            foreach(var s in _subscribers)
                s.ReceiveCommand(command);
        }




        public void ExecuteCurrentRecipe()
        {
            
            var tc = Watchman.Get<HornedAxe>();
            var aspectsInContext = tc.GetAspectsInContext(GetAspects(true));

            RecipeConductor rc =new RecipeConductor(aspectsInContext, Watchman.Get<Stable>().Protag());

            IList<AlternateRecipeExecution> alternateRecipeExecutions = rc.GetAlternateRecipes(Recipe);

           //The first recipe will either be the current recipe, or the alternate recipe which has replaced it.
           //Any others in the list will be additionals that we'll execute afterwards in their own right.

           var primaryRecipeExecution = alternateRecipeExecutions.First();
            var additionalRecipeExecutions = alternateRecipeExecutions.Skip(1);

            
            //Check for possible text refinements based on the aspects in context
            var aspectsInSituation = GetAspects(true);
            TextRefiner tr = new TextRefiner(aspectsInSituation);
            var addNoteCommand = new AddNoteCommand(primaryRecipeExecution.Recipe.Label, tr.RefineString(primaryRecipeExecution.Recipe.Description),new Context(Context.ActionSource.UI));
            ExecuteTokenEffectCommand(addNoteCommand);
             
            RecipeCompletionEffectCommand primaryRecipeCompletionEffectCommand = new RecipeCompletionEffectCommand(primaryRecipeExecution.Recipe,
                primaryRecipeExecution.Recipe.ActionId != Recipe.ActionId, primaryRecipeExecution.Expulsion, primaryRecipeExecution.ToPath);


            Watchman.Get<Stable>().Protag().AddExecutionsToHistory(primaryRecipeCompletionEffectCommand.Recipe.Id, 1);

            primaryRecipeCompletionEffectCommand.Execute(this);

            if (!string.IsNullOrEmpty(primaryRecipeCompletionEffectCommand.Recipe.Ending))
            {
                var ending = Watchman.Get<Compendium>().GetEntityById<Ending>(primaryRecipeCompletionEffectCommand.Recipe.Ending);

                var endGameCommand = new EndGameAtTokenCommand(ending);

                SendCommandToSubscribers(endGameCommand);
            }

            foreach (var c in additionalRecipeExecutions)
            {
                if(c.Recipe.ActionId==Recipe.ActionId)
                    NoonUtility.LogWarning($"{Recipe.ActionId} {Recipe.Id} is trying to start an additional recipe, {c.Recipe.Id}... but it's got the same actionID as the existing situation, so it probably won't spawn successfully.");

                SpawnNewSituation(c.Recipe,c.Expulsion);

            }
        }

        private void SpawnNewSituation(Recipe withRecipe,Expulsion withExpulsion)
        {
            List<Token> stacksToAddToNewSituation = new List<Token>();
            //if there's an expulsion
            if (withExpulsion.Limit > 0)
            {
                //find one or more matching stacks. Important! the limit applies to stacks, not cards. This might need to change.
                AspectMatchFilter filter = new AspectMatchFilter(withExpulsion.Filter);
                var filteredStacks = filter.FilterElementStacks(GetElementTokens(SphereCategory.SituationStorage)).ToList();

                if (filteredStacks.Any() && withExpulsion.Limit > 0)
                {
                    while (filteredStacks.Count > withExpulsion.Limit)
                    {
                        filteredStacks.RemoveAt(filteredStacks.Count - 1);
                    }

                    stacksToAddToNewSituation = filteredStacks;
                }

            }

            var situationCreationCommand = new SituationCreationCommand(withRecipe.ActionId).WithRecipeAboutToActivate(withRecipe.Id);

            situationCreationCommand.TokensToMigrate = stacksToAddToNewSituation;
            var spawnNewTokenCommand = new SpawnNewTokenFromHereCommand(situationCreationCommand,  new Context(Context.ActionSource.SpawningAnchor));

            spawnNewTokenCommand.ExecuteOn(Token);
            
        }


        public void SetToken(Token token)
        {
            _token = token;
        }

        public void OnTokenMoved(TokenLocation toLocation)
        {
            //foreach (var sphere in _spheres)
            //{
            //    sphere.SetReferencePosition(toLocation);
                
            //    Debug.Log($"Reference position x: {toLocation.Anchored3DPosition.x} y: {toLocation.Anchored3DPosition.y} z: {toLocation.Anchored3DPosition.z} in sphere {toLocation.AtSpherePath}");
            //}
        }

        public void StorePopulateDominionCommand(PopulateDominionCommand populateDominionCommand)
        {
            throw new ApplicationException($"No provision for storing a populate dominion command on a situation, but we can't find dominion with identifier {populateDominionCommand.Identifier} on situation {Id}");
        }


        public void OpenAt(TokenLocation location)
    {
           IsOpen = true;
           var changeArgs = new TokenPayloadChangedArgs(this, PayloadChangeType.Update);
           OnChanged?.Invoke(changeArgs);
           Watchman.Get<TabletopManager>().CloseAllSituationWindowsExcept(VerbId);
    }

        public void Close()
        {
            IsOpen = false;

            var changeArgs = new TokenPayloadChangedArgs(this, PayloadChangeType.Update);

            OnChanged?.Invoke(changeArgs);

            DumpUnstartedBusiness();
        }



        public List<IDominion> GetRelevantDominions(StateEnum forState,Type sphereType)
    {
            return new List<IDominion>(_registeredDominions.Where(a=>a.RelevantTo(forState.ToString(),sphereType)));
    }

    public List<Sphere> GetAvailableThresholdsForStackPush(ITokenPayload stack)
    {
        List<Sphere> thresholdsAvailable=new List<Sphere>();
        var thresholds = GetSpheresByCategory(SphereCategory.Threshold);
        foreach (var t in thresholds)

            if (State.IsActiveInThisState(t)
               && !t.CurrentlyBlockedFor(BlockDirection.Inward)
               && t.GetMatchForTokenPayload(stack).MatchType ==SlotMatchForAspectsType.Okay)

                thresholdsAvailable.Add(t);

        return thresholdsAvailable;

    }


        public bool TryPushDraggedStackIntoThreshold(Token token)
        {
            var thresholdSpheres = GetAvailableThresholdsForStackPush(token.Payload);
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
           var retireNotesCommand=new ClearDominionCommand(DominionEnum.Notes,SphereRetirementType.Destructive);
           CommandQueue.AddCommand(retireNotesCommand);
           Continue(0f);
        }


        public void TryStart()
        {
         
            var aspects = GetAspects(true);
            var tc = Watchman.Get<HornedAxe>();
            var aspectsInContext = tc.GetAspectsInContext(aspects);


            var recipe = Watchman.Get<Compendium>().GetFirstMatchingRecipe(aspectsInContext, Verb.Id, Watchman.Get<Stable>().Protag(), false);

            //no recipe found? get outta here
            if (recipe != null)

            {
               var activateRecipeCommand=TryActivateRecipeCommand.ManualRecipeActivation(recipe.Id);
                CommandQueue.AddCommand(activateRecipeCommand);

                //The game might be paused! or the player might just be incredibly quick off the mark
                //so immediately continue with a 0 interval

                Continue(0f);
            }

        }


        public RecipePrediction GetRecipePredictionForCurrentStateAndAspects()
        {
            var aspectsAvailableToSituation = GetAspects(true);

            var aspectsInContext =
                Watchman.Get<HornedAxe>().GetAspectsInContext(aspectsAvailableToSituation);

            RecipeConductor rc = new RecipeConductor(aspectsInContext,Watchman.Get<Stable>().Protag());

            return rc.GetPredictionForFollowupRecipe(Recipe, this);
        }


        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            if (args.Context.Metafictional)
                //it's just a note or something equally cosmetic. Don't update recipe prediction or any of that stuff - it's not only unnecessary, it can also cause weird behaviour.
                return;

            var oldEndingFlavour = CurrentRecipePrediction.SignalEndingFlavour;

            UpdateCurrentRecipePrediction(GetRecipePredictionForCurrentStateAndAspects(),args.Context);
            var newEndingFlavour = CurrentRecipePrediction.SignalEndingFlavour;

            if (oldEndingFlavour!=newEndingFlavour)
                SendCommandToSubscribers(new SignalEndingFlavourCommand(newEndingFlavour));

            foreach (var s in _subscribers)
                s.SituationSphereContentsUpdated(this);
        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            //
        }
        


        public string GetIllumination(string key)
        {
            if (key == NoonConstants.IK_SPONTANEOUS)
            {
                if (Verb.Spontaneous)
                    return "true";
            }

            return string.Empty;
        }

        public void SetIllumination(string key, string value)
        {
    //
        }

        public Timeshadow GetTimeshadow()
        {
            return _timeshadow;

        }

        public void SetTimelessShadow()
        {
            _timeshadow=Timeshadow.CreateTimelessShadow();

        }

        public void SetRecipeActive(Recipe recipeToActivate)
        {
            Recipe = recipeToActivate;
            _timeshadow = new Timeshadow(Recipe.Warmup, Recipe.Warmup, false);
        }

        public void ReduceLifetimeBy(float timeSpent)
        {
            _timeshadow.SpendTime(timeSpent);
        }

        public static bool TextIntendedForDisplay(string descriptiveText)
        {
            if (string.IsNullOrEmpty(descriptiveText))
                return false;

            if (descriptiveText == ".")

                return false;

            return true;
        }
    }

}
