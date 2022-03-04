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
using SecretHistories.Assets.Scripts.Application.Commands.SituationCommands;
using SecretHistories.Assets.Scripts.Application.UI;
using SecretHistories.Commands.TokenEffectCommands;
using SecretHistories.Constants;
using SecretHistories.Core;
using SecretHistories.Events;
using SecretHistories.Infrastructure;
using SecretHistories.Logic;
using SecretHistories;
using SecretHistories.Manifestations;
using SecretHistories.Spheres.Angels;
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


        [Encaust] public string Id { get; protected set; }
        public FucinePath GetAbsolutePath()
        {
            if(_token==null)
                return new NullFucinePath();
            var pathAbove = _token.Sphere.GetAbsolutePath();
            var absolutePath = pathAbove.AppendingToken(this.Id);
            return absolutePath;
        }

        public FucinePath GetWildPath()
        {
            var wildCardPath = FucinePath.Wild();
            
            return wildCardPath.AppendingToken(this.Id);
        }

        public RectTransform GetRectTransform()
        {
            return Token.TokenRectTransform;
        }

        [DontEncaust] public string EntityId => Verb.Id;

        [Encaust]
        public List<AbstractDominion> Dominions => new List<AbstractDominion>(_registeredDominions);

        [DontEncaust] public bool Metafictional => false;


        [Encaust]
        public bool IsOpen { get; private set; }

        [Encaust]
        public List<ISituationCommand> CommandQueue { get; set; } = new List<ISituationCommand>();


        [Encaust] public Dictionary<string, int> Mutations => new Dictionary<string, int>();
        [Encaust] public int Quantity => 1;

        [DontEncaust] public RecipePrediction CurrentRecipePrediction => _currentRecipePrediction;
        [DontEncaust] public SituationState State { get; set; }
        [DontEncaust] public float Warmup => Recipe.Warmup;

        /// <summary>
        /// At time of writing, Label and Description are updated internally when a note is added. This may cause issues if we stop doing that, but 'effective description'
        /// is a fuzzy term that may change its meaning.
        /// </summary>
        [DontEncaust] public string Label { private set; get; }
        /// <summary>
        /// At time of writing, Label and Description are updated internally when a note is added. This may cause issues if we stop doing that, but 'effective description'
        /// is a fuzzy term that may change its meaning.
        /// </summary>
        [DontEncaust] public string Description { private set; get; }

        [DontEncaust] public float IntervalForLastHeartbeat { private set; get; }
        
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
        private readonly List<AbstractDominion> _registeredDominions = new List<AbstractDominion>();
        private readonly HashSet<Sphere> _spheres = new HashSet<Sphere>();
        
        private Timeshadow _timeshadow;
        private Token _token;


        public Situation(Verb verb,string id)
        {
            State = new NullSituationState();
            Id = id;
            Recipe = NullRecipe.Create(); // a newly created situation isn't running a recipe. This will come with TryActiveRecipe, later.
            Verb = verb;
            _timeshadow = new Timeshadow(Recipe.Warmup,
                Recipe.Warmup,
                false);

            HornedAxe hornedAxe = Watchman.Get<HornedAxe>();
            hornedAxe.RegisterSituation(this);
            
        }




        public bool RegisterDominion(AbstractDominion dominionToRegister)
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
            var args=new TokenPayloadChangedArgs(this,PayloadChangeType.Update);
            OnChanged?.Invoke(args);
        }

        public void DetachSphere(Sphere c)
        {
            c.Unsubscribe(this);
            _spheres.Remove(c);
            var args = new TokenPayloadChangedArgs(this, PayloadChangeType.Update);
            OnChanged?.Invoke(args);
        }

        public void ReactToLatestRecipePrediction(RecipePrediction newRecipePrediction,Context context)
        {

            Token.ExecuteTokenEffectCommand(new SignalEndingFlavourCommand(newRecipePrediction.SignalEndingFlavour));

            if (!newRecipePrediction.AddsMeaningfulInformation(_currentRecipePrediction))
                return;

            _currentRecipePrediction = newRecipePrediction;
            NoonUtility.Log($"Situation notification: recipe prediction updated from {_currentRecipePrediction.RecipeId} to {newRecipePrediction.RecipeId}.", 0, VerbosityLevel.Significants);


            var addNoteCommand=new AddNoteToTokenCommand(newRecipePrediction, context);
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

        public Type GetManifestationType(Sphere sphere)
        {
          
            if (sphere.SphereCategory == SphereCategory.Meta)
                return typeof(NullManifestation);

            if (Verb == null)
                return typeof(NullManifestation);

            switch (Verb.Category)
            {
                case VerbCategory.Someone:
                    return typeof(MortalManifestation);
                case VerbCategory.Workstation:
                    return typeof(WorkstationManifestation);
                default:
                    return sphere.GetShabdaManifestation(this);
            }


        }

        public Type GetWindowType()
        {
            if (Verb == null)
                return typeof(SituationWindow); // shouldn't happen, but belt and braces

            switch (Verb.Category)
            {
                case VerbCategory.Someone:
                    return typeof(MortalWindow);
                default:
                    return typeof(SituationWindow);
            }


        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            manifestation.Initialise(this);
        }

        public virtual bool IsValid()
        {
            return true;
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public bool IsPermanent()
        {
            return false;
        }

        public void FirstHeartbeat()
        {
             ExecuteHeartbeat(0f, 0f);
     

            NotifyStateChange();
             NotifyTimerChange();



        }

        public void ExecuteHeartbeat(float seconds, float metaseconds)
        {
            Continue(seconds,metaseconds);
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

        public bool Retire(RetirementVFX VFX)
        {
            var spheresToRetire=new List<Sphere>(_spheres);
                foreach (var c in spheresToRetire)
                {
                    c.Retire(SphereRetirementType.Destructive);
                }

                TokenPayloadChangedArgs args = new TokenPayloadChangedArgs(this, PayloadChangeType.Retirement);
                args.VFX = RetirementVFX.Default;
                OnChanged?.Invoke(args);
                Watchman.Get<HornedAxe>().DeregisterSituation(this);

                return true;
        }

        public void InteractWithIncoming(Token token)
        {
            if (token.IsValidElementStack())
            {
                TryPushDraggedStackIntoThreshold(token);
            }
            else
            {
                //something has gone awryy
                token.CurrentState= new RejectedByTokenState();
            }
        }

        public bool ReceiveNote(INotification notification, Context context)
        {
            //no infinite loops pls
            if (notification.Title == this.Label && notification.Description == this.Description)
            {

                return true;
            }

            if (!Situation.TextIntendedForDisplay(notification.Description))
                return true;

            //At time of writing, these are only used to check whether a note is a meaningful update.
            Label = notification.Title;
            Description = notification.Description;

            var noteElementId = Watchman.Get<Compendium>().GetSingleEntity<Dictum>().NoteElementId;

            var notesDominion = GetRelevantDominions(StateIdentifier, typeof(NotesSphere)).FirstOrDefault();
            if (notesDominion == null)
            {
                NoonUtility.Log($"No notes sphere and no notes dominion found: we won't add note {notification.Title}, then.");
                return false;
            }


            Sphere notesSphere = notesDominion.Spheres.SingleOrDefault();
            if (notesSphere == null)
            {
                //no notes sphere yet exists: create one
                var notesSphereSpec = new SphereSpec(typeof(NotesSphere), $"{nameof(NotesSphere)}");
                notesSphere = notesDominion.TryCreateSphere(notesSphereSpec);
                notesSphere.transform.localScale = Vector3.one; //HACK! there's a bug where opening the window can increase the scale of new notes spheres. This sets it usefully, but I should find a more permanent solution if the issue shows up again
            }

            if (!notification.Additive) //clear out existing notes first
                notesSphere.RetireAllTokens();
                

            var newNoteCommand = new ElementStackCreationCommand(noteElementId, 1);
            newNoteCommand.Illuminations.Add(NoonConstants.TLG_NOTES_TITLE_KEY, notification.Title);
            newNoteCommand.Illuminations.Add(NoonConstants.TLG_NOTES_DESCRIPTION_KEY, notification.Description);
            newNoteCommand.Illuminations.Add(NoonConstants.TLG_NOTES_EMPHASISLEVEL_KEY,notification.EmphasisLevel.ToString());

            var tokenCreationCommand =
                new TokenCreationCommand(newNoteCommand, TokenLocation.Default(notesSphere.GetAbsolutePath()));

            tokenCreationCommand.Execute(context, notesSphere);

            OnChanged?.Invoke(new TokenPayloadChangedArgs(this, PayloadChangeType.Update));

            return true;
        }






        public void ShowNoMergeMessage(ITokenPayload incomingTokenPayload)
        {
            NoonUtility.LogWarning($"Trying to ShowNoMergeMessage for {this.Id}, but we don't know how to do that for a situation.");

        }

        public void SetQuantity(int quantityToLeaveBehind, Context context)
        {
            NoonUtility.LogWarning($"Trying to set the quantity of {this.Id}, but we don't know how to do that for a situation.");

        }

        public void ModifyQuantity(int unsatisfiedChange, Context context)
        {
            NoonUtility.LogWarning($"Trying to modify the quantity of {this.Id}, but we don't know how to do that for a situation.");
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
                    maxToPurge = container.TryPurgeStacks(elementToPurge, maxToPurge);
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
            if(acceptingSphere==null)
            {
                
                NoonUtility.LogWarning($"Situation {Verb.Id} {RecipeId} couldn't tried to accept tokens into sphere category {forSphereCategory}, but couldn't find one. Evicting tokens rather than crashing.");
                foreach(var t in tokens)
                    t.GoAway(context);
            }
                    else
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
            ExecuteCommandsFor(State.Identifier, this);


            NotifyStateChange();
            NotifyTimerChange();
        }

        private SituationState Continue(float seconds,float metaseconds)
        {
            IntervalForLastHeartbeat = seconds;
            ExecuteCommandsFor(State.Identifier,this);
            State.Continue(this);

            return State;
        }

        public void ExecuteCommandsFor(StateEnum forState, Situation situation)
        {
            foreach (var command in new List<ISituationCommand>(CommandQueue))
            {

                if(command.IsObsoleteInState(forState))
                    MarkCommandCompleted(command);

                if (command.IsValidForState(forState))
                {
                    bool executed = command.Execute(situation);
                    if (executed)
                        MarkCommandCompleted(command);
                }
            }
        }

        public void AddCommand(ISituationCommand command)
        {
            CommandQueue.Add(command);
        }

        public void AddCommandsFrom(List<ISituationCommand> existingQueue)
        {
            foreach (var c in existingQueue)
                AddCommand(c);
        }

        public void MarkCommandCompleted(ISituationCommand command)
        {
            CommandQueue.Remove(command);

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
                {
                    dominion.Dismiss();
                }
            }

            OnChanged?.Invoke(new TokenPayloadChangedArgs(this,PayloadChangeType.Update));

            var prediction = GetRecipePredictionForCurrentStateAndAspects();
            ReactToLatestRecipePrediction(prediction,new Context(Context.ActionSource.Unknown));

        }

        public void NotifyTimerChange()
        {
            
            foreach (var subscriber in _subscribers)
                subscriber.TimerValuesChanged(this);
            OnChanged?.Invoke(new TokenPayloadChangedArgs(this, PayloadChangeType.Update));
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
            var notePrimaryRecipeIsExecuting = new Notification(primaryRecipeExecution.Recipe.Label,
                tr.RefineString(primaryRecipeExecution.Recipe.Description),true);

            NoonUtility.Log($"Situation notification: recipe {primaryRecipeExecution.Recipe.Id} is executing and will display description.", 0, VerbosityLevel.Significants);


            var addNoteCommand = new AddNoteToTokenCommand(notePrimaryRecipeIsExecuting, new Context(Context.ActionSource.UI));
            ExecuteTokenEffectCommand(addNoteCommand);
             
            RecipeCompletionEffectCommand primaryRecipeCompletionEffectCommand = new RecipeCompletionEffectCommand(primaryRecipeExecution.Recipe,
                primaryRecipeExecution.Recipe.ActionId != Recipe.ActionId, primaryRecipeExecution.Expulsion, primaryRecipeExecution.ToPath);


            Watchman.Get<Stable>().Protag().AddExecutionsToHistory(primaryRecipeCompletionEffectCommand.Recipe.Id, 1);

            primaryRecipeCompletionEffectCommand.Execute(this);

            if (!string.IsNullOrEmpty(primaryRecipeCompletionEffectCommand.Recipe.Ending))
            {
                var ending = Watchman.Get<Compendium>().GetEntityById<Ending>(primaryRecipeCompletionEffectCommand.Recipe.Ending);

                Watchman.Get<GameGateway>().EndGame(ending, _token);
                return;
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

            var spawnNewTokenCommand = new SpawnNewTokenFromThisOneCommand(situationCreationCommand, FucinePath.Current(),  new Context(Context.ActionSource.JustSpawned));

          if(spawnNewTokenCommand.ExecuteOn(Token))
                situationCreationCommand.LastSituationCreated.AcceptTokens(SphereCategory.SituationStorage,stacksToAddToNewSituation);
            
        }


        public bool ApplyExoticEffect(ExoticEffect exoticEffect)
        {
            if (exoticEffect == ExoticEffect.Purge || exoticEffect == ExoticEffect.BurnPurge)
            {
                Retire(RetirementVFX.Default); //leaving room here for some day transforming with an element-decay like effect, for instance
                return true;
            }

            if (exoticEffect == ExoticEffect.Halt)
            {
                AddCommand(new TryHaltSituationCommand());
                ExecuteHeartbeat(0f, 0f);
                return true;
            }

            return false;
        }

        public void SetToken(Token token)
        {
            _token = token;
        }

        public void OnTokenMoved(TokenLocation toLocation)
        {
            NotifySpheresChanged(new Context(Context.ActionSource.SphereReferenceLocationChanged));
        }

        public void NotifySpheresChanged(Context context)
        {
            List<Sphere> spheres = new List<Sphere>();
            foreach (var d in Dominions)
            {
                spheres.AddRange(d.Spheres);
            }

            foreach (var sphere in spheres)
            {
                SphereChangedArgs args;
                args = new SphereChangedArgs(sphere, context);
                sphere.NotifySphereChanged(args);
            }

        }

        public void StorePopulateDominionCommand(PopulateDominionCommand populateDominionCommand)
        {
            throw new ApplicationException($"No provision for storing a populate dominion command on a situation, but we can't find dominion with identifier {populateDominionCommand.Identifier} on situation {Id}");
        }

        private void Open()
        {
        if(_token==null)
            OpenAt(TokenLocation.Default(GetAbsolutePath()));
        else
            OpenAt(_token.Location);
        }


        public void OpenAt(TokenLocation location)
    {
           IsOpen = true;
           var changeArgs = new TokenPayloadChangedArgs(this, PayloadChangeType.Opening);
           OnChanged?.Invoke(changeArgs);
           var meniscate= Watchman.Get<Meniscate>();
           meniscate.CloseAllSituationWindowsExcept(VerbId);
           _token?.Emphasise(); //does nothing for verbs, important for someones
        }

        public void Close()
        {
            IsOpen = false;
            
            var changeArgs = new TokenPayloadChangedArgs(this, PayloadChangeType.Closing);

            OnChanged?.Invoke(changeArgs);
            DumpUnstartedBusiness();
            _token?.Understate(); //does nothing for verbs, important for someones
        }



        public List<AbstractDominion> GetRelevantDominions(StateEnum forState,Type sphereType)
    {
            return new List<AbstractDominion>(_registeredDominions.Where(a=>a.RelevantTo(forState.ToString(),sphereType)));
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
                    incumbent.GoAway(new Context(Context.ActionSource.PlaceInThresholdUsurpedByIncomer));
                 
                return occupiedThresholdSphere.TryAcceptToken(token, new Context(Context.ActionSource.PlayerDrag));
            }


            return false;

        }



        public void DumpUnstartedBusiness()
        {
            var verbThresholdsDominion = GetRelevantDominions(StateEnum.Unstarted, typeof(ThresholdSphere)).SingleOrDefault();
            if(verbThresholdsDominion!=null)
            {
              var verbThresholds = new List<Sphere>(verbThresholdsDominion.Spheres);
                foreach (var vt in verbThresholds)
                {
                    if(!vt.Defunct)
                        vt.EvictAllTokens(Context.Unknown());
                }
            }
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
            AddCommand(new EvictOutputCommand());
            AddCommand(new ConcludeCommand());
           Continue(0f,0f);
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
                AddCommand(activateRecipeCommand);

                //The game might be paused! or the player might just be incredibly quick off the mark
                //so immediately continue with a 0 interval

                Continue(0f,0f);
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


        public void OnSphereChanged(SphereChangedArgs args)
        {
       
        }

        private bool ChangesAreMetafictional(SphereContentsChangedEventArgs args)
        {
          if(args.TokenAdded != null && !args.TokenAdded.Payload.Metafictional && args.TokenAdded.Payload.IsValid())
                return false;
          if (args.TokenRemoved != null && !args.TokenRemoved.Payload.Metafictional && args.TokenRemoved.Payload.IsValid())
              return false;
          if (args.TokenChanged != null && !args.TokenChanged.Payload.Metafictional && args.TokenChanged.Payload.IsValid())
              return false;

          return true;

        }

        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            if (ChangesAreMetafictional(args))
                //it's just a note or something equally cosmetic. Don't update recipe prediction or any of that stuff - it's not only unnecessary, it can also cause weird behaviour.
                return;
            
            // if a token has just been added to a situation -eg by dropping a token on a verb, or double-click - to - send arriving - then open this situation
            if (args.Sphere.SphereCategory == SphereCategory.Threshold &&
                !args.Sphere.HasAngel(typeof(GreedyAngel)) && //a token going to a greedy sphere shouldn't trigger a situation opening
                 args.TokenAdded != null &&
                args.TokenRemoved == null && 
                args.TokenChanged == null)
            {
                Open();
            }

            if (args.Sphere.SphereCategory == SphereCategory.Output &&
                args.TokenRemoved != null &&
                !args.Sphere.Tokens.Any())
            {
                //we've just removed the last token from the output. Conclude.
                Conclude();
            }

            if (State.UpdatePredictionDynamically)
            {
                var latestPrediction=GetRecipePredictionForCurrentStateAndAspects();
                ReactToLatestRecipePrediction(latestPrediction, args.Context);
            }
                

            foreach (var s in _subscribers)
                s.SituationSphereContentsUpdated(this);

            OnChanged?.Invoke(new TokenPayloadChangedArgs(this, PayloadChangeType.Update));
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
