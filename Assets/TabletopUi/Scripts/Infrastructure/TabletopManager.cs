﻿#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.Core.Services;
using Assets.Logic;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts.TokenContainers;
using Assets.TabletopUi.Scripts.UI;
using Noon;
using TabletopUi.Scripts.Elements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

namespace Assets.CS.TabletopUI {
    public class TabletopManager : MonoBehaviour,ISettingSubscriber
    {

        [SerializeField] private EndGameAnimController _endGameAnimController;

        [Header("Tabletop")] [SerializeField] public TabletopSphere _tabletop;
        [SerializeField] TabletopBackground tabletopBackground;

        [SerializeField] public WindowsSphere WindowsSphere;
        
        [Header("Detail Windows")] [SerializeField]
        private AspectDetailsWindow aspectDetailsWindow;

        [SerializeField] private TokenDetailsWindow tokenDetailsWindow;
        [SerializeField] private CardHoverDetail cardHoverDetail;


        [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("mapContainsTokens")]
        public MapSphere MapSphere;

        [SerializeField] TabletopBackground mapBackground;
        [SerializeField] MapAnimation mapAnimation;

        [Header("Drag & Window")] [SerializeField]
        private RectTransform draggableHolderRectTransform;

        [SerializeField] private ScrollRect tableScroll;
        [SerializeField] public GameObject _dropZoneTemplate;


        [Header("Status Bar & Notes")] [SerializeField]
        private StatusBar StatusBar;

        [SerializeField] private BackgroundMusic backgroundMusic;

        [SerializeField] private Notifier _notifier;
        [SerializeField] private AutosaveWindow _autosaveNotifier;
        [SerializeField] private ElementOverview _elementOverview;
        
        

        private bool disabled;
        private bool _initialised;






        public enum NonSaveableType
        {
            Drag, // Cannot save because held card gets lost
            Mansus, // Cannot save by design
            Greedy, // Cannot save during Magnet grab (spec fix for #1253)
            WindowAnim, // Cannot save during situation window open
            NumNonSaveableTypes
        };

        static private bool[] isInNonSaveableState = new bool[(int) NonSaveableType.NumNonSaveableTypes];

        private Situation mansusSituation;
        //private Vector2 preMansusTabletopPos; // Disabled cause it looks jerky -Martin

        public static bool IsInMansus()
        {
            return isInNonSaveableState[(int) NonSaveableType.Mansus];
        }

        private float
            housekeepingTimer = 0.0f; // Now a float so that we can time autosaves independent of Heart.Beat - CP

        private float AUTOSAVE_INTERVAL = 300.0f;

        private static bool highContrastMode = false;
        private static bool accessibleCards = false;
        private List<string> currentDoomTokens = new List<string>();

        public void ForceAutosave() // Useful for forcing autosave to happen at tricky moments for debugging - CP
        {
            housekeepingTimer = AUTOSAVE_INTERVAL;
        }



        public async void Update()
        {
            if (disabled)
                return; //we've had to shut down because of a critical error

            if (!_initialised)
                return; //still setting up


            housekeepingTimer += Time.deltaTime;
            if (housekeepingTimer >= AUTOSAVE_INTERVAL && IsSafeToAutosave()
            ) // Hold off autsave until it's safe, rather than waiting for the next autosave - CP
            {
                housekeepingTimer = 0.0f;

                var saveTask = SaveGameAsync(true, SourceForGameState.DefaultSave);
                var success = await saveTask;

                if (!success)
                    housekeepingTimer = AUTOSAVE_INTERVAL - 5.0f;

            }
        }


        private void AppealToConscience()
        {
            string appealToConscienceLocation = Application.streamingAssetsPath + "/edition/please_buy_our_game.txt";
            if (File.Exists(appealToConscienceLocation))
            {
                var content = File.ReadLines(appealToConscienceLocation);
                DateTime expiry = Convert.ToDateTime(content.First());
                if (DateTime.Today > expiry)
                {
                    _notifier.ShowNotificationWindow("ERROR - PLEASE UPDATE GAME", @"CRITICAL UPDATE REQUIRED");
                    return;
                }
            }
        }


        void Awake()
        {
            var registry = new Registry();
            registry.Register(this);
        }

        void Start()
        {
            try
            {

                //AppealToConscience();
            var registry = new Registry();
            
          
            //register everything used gamewide
            SetupServices(registry);

            //we hand off board functions to individual controllers
            InitialiseSubControllers(
                _endGameAnimController
            );

            InitialiseListeners();

            _initialised = true;


            if (Registry.Get<StageHand>().SourceForGameState == SourceForGameState.NewGame)
            {
                BeginNewGame();
            }
            else
            {
                LoadExistingGame(Registry.Get<StageHand>().SourceForGameState);
            }
            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }

        }

        /// <summary>
        /// if a game exists, load it; otherwise, create a fresh state and setup
        /// </summary>
        private void LoadExistingGame(SourceForGameState source)
        {
            
            bool isSaveCorrupted = false;
            bool shouldContinueGame;
            try
            {
                shouldContinueGame = Registry.Get<GameSaveManager>().DoesGameSaveExist() && Registry.Get<GameSaveManager>().IsSavedGameActive(source);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load game (see exception for details)");
                Debug.LogException(e);
                shouldContinueGame = false;
                isSaveCorrupted = true;
            }

            LoadGame(source);

            if (!shouldContinueGame || isSaveCorrupted)
            {
                _notifier.ShowSaveError(true);
                GameSaveManager.saveErrorWarningTriggered = true;
            }


            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
                { ControlPriorityLevel = 2, GameSpeed = GameSpeed.Paused, WithSFX = false });


            Registry.Get<LocalNexus>().UILookAtMeEvent.Invoke(typeof(SpeedControlUI));
            _elementOverview.UpdateDisplay(); //show initial correct count of everything we've just loaded


        }



    private void BeginNewGame()
        {
            SetupNewBoard();
            var populatedCharacter =
                Registry.Get<Character>(); //should just have been set above, but let's keep this clean
            populatedCharacter.Reset(populatedCharacter.ActiveLegacy,null);
            Registry.Get<Compendium>().SupplyLevers(populatedCharacter);
     Registry.Get<StageHand>().ClearRestartingGameFlag();
        }

        private void InitialiseSubControllers(EndGameAnimController endGameAnimController) {

            endGameAnimController.Initialise();
        }

        private void InitialiseListeners() {
            // Init Listeners to pre-existing DisplayHere Objects
            mapBackground.onDropped += HandleOnMapBackgroundDropped;
        }

        private void OnDestroy() {
            // Static event so make sure to de-init once this object is destroyed
            mapBackground.onDropped -= HandleOnMapBackgroundDropped;
        }




        private void SetupServices(Registry registry)
        {


            var choreographer = new Choreographer(_tabletop);
            registry.Register(choreographer);


            registry.Register<INotifier>(_notifier);

            
  

            //element overview needs to be initialised with
            // - legacy - in case we're displaying unusual info
            // stacks catalogue - so it can subscribe for notifications re changes
            Legacy legacy = Registry.Get<Character>().ActiveLegacy;

            //this shouldn't happen, but here's a guard clause in case it does.
            if(legacy==null)
                Registry.Get<StageHand>().EndingScreen();

            _elementOverview.Initialise(legacy,  Registry.Get<Compendium>());
            Registry.Get<SphereCatalogue>().Subscribe(_elementOverview);
            tabletopBackground.ShowTabletopFor(legacy);

  

        }



        public void SetupNewBoard() {


     
            Character _character = Registry.Get<Character>();
            if(_character.ActiveLegacy==null)
                throw new ApplicationException("Trying to set up a new board for a character with no chosen legacy. Even fresh characters should have a legacy when created, but this code has always been hinky.");

            IVerb v = Registry.Get<Compendium>().GetEntityById<BasicVerb>(_character.ActiveLegacy.StartingVerbId);
            SituationCreationCommand command = new SituationCreationCommand(v, NullRecipe.Create(), StateEnum.Unstarted,
                new TokenLocation(0f,0f,-100f, _tabletop.GetPath()));
            var situation = Registry.Get<SituationBuilder>().CreateSituation(command);
            
            situation.ExecuteHeartbeat(0f);


            SetStartingCharacterInfo(_character.ActiveLegacy);

            ProvisionStartingElements(_character.ActiveLegacy);

            StatusBar.UpdateCharacterDetailsView(Registry.Get<Character>());

            
            _notifier.ShowNotificationWindow(_character.ActiveLegacy.Label, _character.ActiveLegacy.StartDescription);
        }

        private void SetStartingCharacterInfo(Legacy chosenLegacy)
		{
            Character newCharacter = Registry.Get<Character>();
            newCharacter.Name = Registry.Get<ILocStringProvider>().Get("UI_CLICK_TO_NAME");
           // Registry.Retrieve<Chronicler>().CharacterNameChanged(NoonConstants.DEFAULT_CHARACTER_NAME);//so we never see a 'click to rename' in future history
            newCharacter.Profession = chosenLegacy.Label;
        }



        public void ProvisionStartingElements(Legacy chosenLegacy) {
            AspectsDictionary startingElements = new AspectsDictionary();
            startingElements.CombineAspects(chosenLegacy.Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.

            foreach (var e in startingElements)
            {
                var context = new Context(Context.ActionSource.Loading);

                Token token = _tabletop.ProvisionElementStackToken(e.Key, e.Value, Source.Existing(),context,Element.EmptyMutationsDictionary());
                Registry.Get<Choreographer>().PlaceTokenOnTableAtFreePosition(token, context);
            }
        }


        public int PurgeElement(string elementId, int maxToPurge)
        {
            var compendium = Registry.Get<Compendium>();

            Element elementToPurge = compendium.GetEntityById<Element>(elementId);
            
          maxToPurge-=_tabletop.TryPurgeStacks(elementToPurge, maxToPurge);

           var situationsCatalogue = Registry.Get<SituationsCatalogue>();
           foreach (var s in situationsCatalogue.GetRegisteredSituations())
           {
               if (maxToPurge <= 0)
                   return maxToPurge;
               else
                    maxToPurge -= s.TryPurgeStacks(elementToPurge, maxToPurge);

           }

           return maxToPurge;
        }

        public void HaltVerb(string toHaltId, int maxToHalt)
        {
            var situationsCatalogue = Registry.Get<SituationsCatalogue>();
            int i = 0;
            //Halt the verb if the actionId matches BEARING IN MIND WILDCARD

            if (toHaltId.Contains('*'))
            {
                string wildcardToDelete = toHaltId.Remove(toHaltId.IndexOf('*'));

                foreach (var s in situationsCatalogue.GetRegisteredSituations())
                {
                    if (s.Verb.Id.StartsWith(wildcardToDelete))
                    {
                        s.Halt();
                        i++;
                    }

                    if (i >= maxToHalt)
                        break;
                }
            }

            else
            {
                foreach (var s in situationsCatalogue.GetRegisteredSituations())
                {
                    if (s.Verb.Id == toHaltId.Trim())
                    {
                        s.Halt();
                        i++;
                    }
                    if (i >= maxToHalt)
                        break;
                }
            }
        }

        public void DeleteVerb(string toDeleteId, int maxToDelete)
        {
            var situationsCatalogue = Registry.Get<SituationsCatalogue>();
            int i = 0;
            //Delete the verb if the actionId matches BEARING IN MIND WILDCARD

            if (toDeleteId.Contains('*'))
            {
                string wildcardToDelete = toDeleteId.Remove(toDeleteId.IndexOf('*'));

                foreach (var s in situationsCatalogue.GetRegisteredSituations())
                {
                    if (s.Verb.Id.StartsWith(wildcardToDelete))
                    {
                        s.Retire();
                        i++;
                    }

                    if (i >= maxToDelete)
                        break;
                }
            }

            else
                {
                    foreach (var s in situationsCatalogue.GetRegisteredSituations())
                    {
                 if (s.Verb.Id == toDeleteId.Trim())
                        {
                            s.Retire();
                            i++;
                        }
                        if (i >= maxToDelete)
                            break;
                    }
                }
        }


        public async void EndGame(Ending ending, Token _anchor)
		{
			NoonUtility.Log("TabletopManager.EndGame()");

            
            var character = Registry.Get<Character>();
            var chronicler = Registry.Get<Chronicler>();

            chronicler.ChronicleGameEnd(Registry.Get<SituationsCatalogue>().GetRegisteredSituations(), Registry.Get<SphereCatalogue>().GetSpheres(),ending);
            character.Reset(null,ending);
            


            var saveTask = Registry.Get<GameSaveManager>().SaveActiveGameAsync(new InactiveTableSaveState(Registry.Get<MetaInfo>()), Registry.Get<Character>(),SourceForGameState.DefaultSave);
            var result = await saveTask;

        
            _endGameAnimController.TriggerEnd(_anchor, ending);
        }


      

        public void LoadGame(SourceForGameState gameStateSource) {
            Compendium compendium = Registry.Get<Compendium>();


            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
                {ControlPriorityLevel = 1, GameSpeed = GameSpeed.Paused, WithSFX = false});
            Registry.Get<LocalNexus>().UILookAtMeEvent.Invoke(typeof(SpeedControlUI));
            try
            {

                Registry.Get<GameSaveManager>().LoadTabletopState(gameStateSource, _tabletop);
                //saveGameManager.ImportHashedSaveToState(_tabletop, null, htSave);

                StatusBar.UpdateCharacterDetailsView(Registry.Get<Character>());

				// Reopen any windows that were open at time of saving. I think there can only be one, but checking all for robustness - CP
				var allSituationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();
	            foreach (var s in allSituationControllers)
				{
					if (s.IsOpen)
					{
					s.OpenAtCurrentLocation();
					}
				}

	            _notifier.ShowNotificationWindow(Registry.Get<ILocStringProvider>().Get("UI_LOADEDTITLE"), Registry.Get<ILocStringProvider>().Get("UI_LOADEDDESC"));
            }
            catch (Exception e)
            {
                _notifier.ShowNotificationWindow(Registry.Get<ILocStringProvider>().Get("UI_LOADFAILEDTITLE"), Registry.Get<ILocStringProvider>().Get("UI_LOADFAILEDDESC"));
                Debug.LogError("Failed to load game (see exception for details)");
                Debug.LogException(e, this);
            }

            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
                {ControlPriorityLevel = 1, GameSpeed = GameSpeed.Paused, WithSFX = false});

Registry.Get<LocalNexus>().UILookAtMeEvent.Invoke(typeof(SpeedControlUI));

            var activeLegacy = Registry.Get<Character>().ActiveLegacy;

            _elementOverview.Initialise(activeLegacy, compendium);

                
            Registry.Get<SphereCatalogue>().Subscribe(_elementOverview);
            tabletopBackground.ShowTabletopFor(activeLegacy);

        }



        public async Task<bool> SaveGameAsync(bool withNotification, SourceForGameState source)
        {
            return false;
            if (!IsSafeToAutosave())
            {
                NoonUtility.Log("Unsafe to autosave: returning", 0,VerbosityLevel.SystemChatter);
                return false;
            }

            if (withNotification && _autosaveNotifier != null)
            {
                NoonUtility.Log("Displaying autosave notification", 0, VerbosityLevel.SystemChatter);

                //_notifier.ShowNotificationWindow("SAVED THE GAME", "BUT NOT THE WORLD");
                _autosaveNotifier.SetDuration(3.0f);
                _autosaveNotifier.Show();
            }


			Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs{ControlPriorityLevel = 3,GameSpeed = GameSpeed.Paused,WithSFX = false});
            NoonUtility.Log("Paused game for saving", 0,VerbosityLevel.SystemChatter);


            try
            {
	            ITableSaveState tableSaveState=new TableSaveState(_tabletop.GetElementStacks(), Registry.Get<SituationsCatalogue>().GetRegisteredSituations(),Registry.Get<MetaInfo>());
                 var   saveTask = Registry.Get<GameSaveManager>().SaveActiveGameAsync(tableSaveState,  Registry.Get<Character>(), source);
                 NoonUtility.Log("Beginning save", 0,VerbosityLevel.SystemChatter);
               bool    success = await saveTask;
                NoonUtility.Log($"Save status: {success}", 0,VerbosityLevel.SystemChatter);

            }
            catch (Exception e)
            {
	            _notifier.ShowSaveError(true);
	            GameSaveManager.saveErrorWarningTriggered = true;
	            Debug.LogError("Failed to save game (see exception for details)");
	            Debug.LogException(e);
            }

            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.DeferToNextLowestCommand, WithSFX = false });
            NoonUtility.Log("Unpausing game after saving",0,VerbosityLevel.SystemChatter);




			if (GameSaveManager.saveErrorWarningTriggered)	// Do a full pause after resuming heartbeat (to update UI, SFX, etc)
			{
                NoonUtility.Log("Triggering save error warning", 0,VerbosityLevel.SystemChatter);

                // only pause if we need to (since it triggers sfx)
                Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
                    {ControlPriorityLevel = 1, GameSpeed = GameSpeed.Paused, WithSFX = false});

				GameSaveManager.saveErrorWarningTriggered = false;	// Clear after we've used it
			}

            return true;
        }

        public async void LeaveGame()
        {

            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel = 3, GameSpeed = GameSpeed.Paused, WithSFX = false });
            var saveTask = SaveGameAsync(true, SourceForGameState.DefaultSave);

            var success = await saveTask;


            if (success)
            {
                Registry.Get<StageHand>().MenuScreen();
            }
            else
            {
                // Save failed, need to let player know there's an issue
                // Autosave would wait and retry in a few seconds, but player is expecting results NOW.
                Registry.Get<LocalNexus>().ToggleOptionsEvent.Invoke();
                Registry.Get<Assets.Core.Interfaces.INotifier>().ShowSaveError(true);
            }

        }

        
        public HashSet<AnchorAndSlot> FillTheseSlotsWithFreeStacks(HashSet<AnchorAndSlot> slotsToFill) {
            var unprocessedSlots = new HashSet<AnchorAndSlot>();

                //uncomment and rework
            //var choreo = Registry.Get<Choreographer>();
            //SituationController sit;

            //foreach (var tokenSlotPair in slotsToFill) {
            //    if (NeedToFillSlot(tokenSlotPair) == false)
            //        continue; // Skip it, we don't need to fill it

            //    var stack = FindStackForSlotSpecificationOnTabletop(tokenSlotPair.Threshold.GoverningSlotSpecification) as ElementStackToken;

            //    if (stack != null) {
            //        stack.SplitAllButNCardsToNewStack(1, new Context(Context.ActionSource.GreedySlot));
            //        choreo.MoveElementToSituationSlot(stack, tokenSlotPair, choreo.ElementGreedyAnimDone);
            //        continue; // we found a stack, we're done here
            //    }

            //    stack = FindStackForSlotSpecificationInSituations(tokenSlotPair.Threshold.GoverningSlotSpecification, out sit) as ElementStackToken;

            //    if (stack != null) {
            //        stack.SplitAllButNCardsToNewStack(1, new Context(Context.ActionSource.GreedySlot));
            //        choreo.PrepareElementForGreedyAnim(stack, sit.situationAnchor as VerbAnchor); // this reparents the card so it can animate properly
            //        choreo.MoveElementToSituationSlot(stack, tokenSlotPair, choreo.ElementGreedyAnimDone);
            //        continue; // we found a stack, we're done here
            //    }

            //    unprocessedSlots.Add(tokenSlotPair);
           // }

            return unprocessedSlots;
        }

        private bool NeedToFillSlot(AnchorAndSlot anchorSlotPair) {
            //rework, move internal

            //if (tokenSlotPair.Token.Equals(null))
            //    return false; // It has been destroyed
            //if (tokenSlotPair.Token.Defunct)
            //    return false;
            //if (!tokenSlotPair.Token.SituationController.IsOngoing)
            //    return false;
            //if (tokenSlotPair.Threshold.Equals(null))
            //    return false; // It has been destroyed
            //if (tokenSlotPair.Threshold.Defunct)
            //    return false;
            //if (tokenSlotPair.Threshold.IsBeingAnimated)
            //    return false; // We're animating something into the slot.
            //if (tokenSlotPair.Threshold.GetElementStackInSlot() != null)
            //    return false; // It is already filled
            //if (tokenSlotPair.Threshold.GoverningSlotSpecification==null || !tokenSlotPair.Threshold.GoverningSlotSpecification.Greedy)
            //    return false; //it's not greedy any more; sometimes if we have a recipe with a greedy slot followed by a recipe with a non-greedy slot, the behaviour carries over for the moment the recipe changes

            return true;
        }

        private Token FindStackForSlotSpecificationOnTabletop(SlotSpecification slotSpec) {

            var rnd = new Random();
            var tokens = _tabletop.GetElementTokens().OrderBy(x=>rnd.Next());

            foreach (var token in tokens)
                if (CanPullCardToGreedySlot(token, slotSpec))
                {

                    if (token._currentlyBeingDragged)
                    {
                        token.SetXNess(TokenXNess.DivertedByGreedySlot);
                        token.FinishDrag();
                    }
                
                    return token;
                }

            
            return null;
        }

        private bool CanPullCardToGreedySlot(Token stackToken, SlotSpecification slotSpec)
        {
            if (slotSpec == null)
                return false; //We were seeing NullReferenceExceptions in the Unity analytics from the bottom line; stack is referenced okay so it shouldn't be stack, so probably a null slotspec is being specified somewhere

            if (stackToken == null) //..but just in case.
                return false;

            if (stackToken.Defunct)
                return false; // don't pull defunct cards
            else if (stackToken.IsInMotion)
                return false; // don't pull animated cards


            var allowExploits = Registry.Get<Config>().GetConfigValueAsInt(NoonConstants.BIRDWORMSLIDER);
            if (allowExploits!=null || allowExploits > 0)
            {
                Debug.Log("exploits on");
                if (stackToken._currentlyBeingDragged)
                    return false; // don't pull cards being dragged if Worm is set On}
            }
            

            return slotSpec.GetSlotMatchForAspects(stackToken.ElementStack.GetAspects()).MatchType == SlotMatchForAspectsType.Okay;
        }

     


		public void CloseAllDetailsWindows()
		{
			if (aspectDetailsWindow != null)
				aspectDetailsWindow.Hide();
			if (tokenDetailsWindow != null)
				tokenDetailsWindow.Hide();
		}

        public void CloseAllSituationWindowsExcept(string exceptVerbId) {
            var situations = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

            foreach (var s in situations)
            {
                if (s.Verb.Id != exceptVerbId)
                    s.Close();
            }
        }

        public bool IsSituationWindowOpen() {
	        var situationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();
	        return situationControllers.Any(c => c.IsOpen);
        }

        public void SetHighlightedElement(string elementId, int quantity = 1)
        {
            var enableAccessibleCards =
                Registry.Get<Config>().GetConfigValueAsInt(NoonConstants.ACCESSIBLECARDS);

            if (enableAccessibleCards==null || enableAccessibleCards==0)
		        return;
	        if (elementId == null)
	        {
		        cardHoverDetail.Hide();
		        return;
	        }
	        cardHoverDetail.Populate(elementId, quantity);
	        cardHoverDetail.Show();
        }
        
        void HandleOnMapBackgroundDropped(PointerEventData eventData)
        {
            var stack = eventData.pointerDrag.GetComponent<Token>();

            if (stack!=null) {

                stack.SetXNess(TokenXNess.DroppedOnTableContainer);
                MapSphere.DisplayHere(stack, new Context(Context.ActionSource.PlayerDrag));

                SoundManager.PlaySfx("CardDrop");
            }
        }

        public void DecayStacksOnTable(float interval) {
            var decayingStacks = _tabletop.GetElementStacks().Where(s => s.Decays);

            foreach (var d in decayingStacks)
                d.Decay(interval);
        }


        public void DecayStacksInResults(float interval)
        {
            var situations = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

            foreach (var s in situations)
            {
                s.TryDecayOutputContents(interval);
            }


        }


		static public void RequestNonSaveableState( NonSaveableType type, bool forbidden )
		{
			// This allows multiple systems to request overlapping NonSaveableStates - CP
			// Removed the counter, as it kept creeping up (must be a loophole if a drag is aborted oddly)
			// For safety I've changed it to array of separate flags (so you can drag in the Mansus without enabled autosave)
			// and added a failsafe in the update, which flushes the Drag flag whenever nothing is held (rather than relying on catching all exit points)
			Debug.Assert( type<NonSaveableType.NumNonSaveableTypes, "Bad nonsaveable type" );
			isInNonSaveableState[(int)type] = forbidden;
		}

		static public void FlushNonSaveableState()	// For use when we absolutely, definitely want to restore autosave permission - CP
		{
			for (int i=0; i<(int)NonSaveableType.NumNonSaveableTypes; i++)
			{
				isInNonSaveableState[i] = false;
			}
		}

		static public bool IsSafeToAutosave()
		{
			for (int i=0; i<(int)NonSaveableType.NumNonSaveableTypes; i++)
			{
				if (isInNonSaveableState[i])
					return false;
			}
			return true;
		}

        

		protected void SetAutosaveInterval( float minutes )
		{
			AUTOSAVE_INTERVAL = minutes * 60.0f;
		}



		public static void SetHighContrast( bool on )
		{
			highContrastMode = on;
   //         Registry.Retrieve<Concursum>().CultureChangedEvent.Invoke();
			//LanguageManager.LanguageChangeHasOccurred();	// Fire language change to recreate all text, which will also apply contrast adjustments - CP 
		}

		public static bool GetHighContrast()
		{
			return highContrastMode;
		}

		public static void SetAccessibleCards( bool on )
		{
			accessibleCards = on;
		}





        public static bool GetAccessibleCards()
		{
			return accessibleCards;
		}



        public void SignalImpendingDoom(Token situationToken) {
            if(!currentDoomTokens.Contains(situationToken.Verb.Id))
                currentDoomTokens.Add(situationToken.Verb.Id);
            backgroundMusic.PlayImpendingDoom();
        }


        public void NoMoreImpendingDoom(Token situationToken)
        {
            if (currentDoomTokens.Contains(situationToken.Verb.Id))
                currentDoomTokens.Remove(situationToken.Verb.Id);
            if(!currentDoomTokens.Any())
                backgroundMusic.NoMoreImpendingDoom();
        }

		static private float cardPingLastTriggered = 0.0f;

		public void HighlightAllStacksForSlotSpecificationOnTabletop(SlotSpecification slotSpec)
		{
			float time = Time.realtimeSinceStartup;
			if (time > cardPingLastTriggered + 1.0f)	// Don't want to trigger these within a second of the last trigger, otherwise they stack up too much
			{
				cardPingLastTriggered = time;

                var stacks = FindAllElementTokenssForSlotSpecificationOnTabletop(slotSpec);

				foreach (var stack in stacks)
				{
					ShowFXonToken("FX/CardPingEffect", stack.transform);
				}
			}
		}

      

        public void GroupAllStacks()
        {
	        var stacks = _tabletop.GetElementTokens();
	        var groups = stacks.OfType<ElementStack>()
		        .GroupBy(e => e.EntityWithMutationsId, e => e)
		        .Select(group => group.OrderByDescending(e => e.Quantity).ToList());
	        
	        foreach (var group in groups)
	        {
		        var primaryStack = group.First();
		        
		        foreach (var stack in group.Skip(1))
			        if (primaryStack.CanMergeWith(stack))
			        {
				        primaryStack.AcceptIncomingStackForMerge(stack);
			        }
	        }
   
        }

		private List<Token> FindAllElementTokenssForSlotSpecificationOnTabletop(SlotSpecification slotSpec) {
			var stackList = new List<Token>();
			var stackTokens = _tabletop.GetElementTokens();

			foreach (var stackToken in stackTokens) {

				if (CanPullCardToGreedySlot(stackToken, slotSpec))
					stackList.Add(stackToken);
			}

			return stackList;
		}

		private void ShowFXonToken(string name, Transform parent) {
			var prefab = Resources.Load(name);

			if (prefab == null)
				return;

			var obj = Instantiate(prefab) as GameObject;

			if (obj == null)
				return;

			obj.transform.SetParent(parent);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.identity;
			obj.gameObject.SetActive(true);
		}

		private void OnGUI()
		{
#if UNITY_EDITOR
			// Extra tools for debugging autosave.

			// Toggle to simulate bad save
			if (GUI.Button( new Rect(Screen.width * 0.5f - 300f, 10f, 180f, 20f), "Simulate bad save: " + (GameSaveManager.simulateBrokenSave?"ON":"OFF") ))
			{
				GameSaveManager.simulateBrokenSave = !GameSaveManager.simulateBrokenSave;		// Click
			}

			// Counter to show time to next autosave. Click it to reduce to a five second countdown
			if (GUI.Button( new Rect(Screen.width * 0.5f - 100f, 10f, 150f, 20f), "Autosave in " + (int)(AUTOSAVE_INTERVAL-housekeepingTimer) ))
			{
				housekeepingTimer = AUTOSAVE_INTERVAL - 5f;		// Click
			}

			if (!IsSafeToAutosave())
			{
				GUI.TextArea( new Rect(Screen.width * 0.5f + 50f, 10f, 70f, 20f), "BLOCKED" );
			}

#endif

		}


        public void WhenSettingUpdated(object newValue)
        {
            SetAutosaveInterval(newValue is float ? (float)newValue : 0);
        }

    }


}
