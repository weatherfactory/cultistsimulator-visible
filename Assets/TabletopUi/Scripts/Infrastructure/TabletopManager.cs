#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.Core.Services;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Logic;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.UI;
using Noon;
using OrbCreationExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.WSA;
using Random = System.Random;

namespace Assets.CS.TabletopUI {
    public class TabletopManager : MonoBehaviour {

        [Header("Game Control")]
        [SerializeField]
        private Heart _heart;
        [SerializeField]
        private SpeedController _speedController;
        [SerializeField]
        private CardAnimationController _cardAnimationController;
        [SerializeField]
        private EndGameAnimController _endGameAnimController;

        [Header("Tabletop")]
        [SerializeField]
        public TabletopTokenContainer _tabletop;
        [SerializeField]
        private Limbo Limbo;

        [Header("Mansus Map")]
        [SerializeField]
        private MapController _mapController;
        [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("mapContainsTokens")]
        public MapTokenContainer mapTokenContainer;
        [SerializeField]
        TabletopBackground mapBackground;
        [SerializeField]
        MapAnimation mapAnimation;

        [Header("Drag & Window")]
        [SerializeField]
        private RectTransform draggableHolderRectTransform;
        [SerializeField]
        Transform tableLevelTransform;
        [SerializeField]
        Transform windowLevelTransform;
        [SerializeField] private ScrollRect tableScroll;

        [Header("Options Bar & Notes")]
        [SerializeField]
        private StatusBar StatusBar;

        [SerializeField]
        private DebugTools debugTools;
        [SerializeField]
        private BackgroundMusic backgroundMusic;
        [SerializeField]
        private HotkeyWatcher _hotkeyWatcher;

        [SerializeField]
        private Notifier _notifier;
		[SerializeField]
        private AutosaveWindow _autosaveNotifier;
        [SerializeField]
        private OptionsPanel _optionsPanel;
        [SerializeField]
        private ElementOverview _elementOverview;

        private SituationBuilder _situationBuilder;

		public enum NonSaveableType
		{
			Drag,		// Cannot save because held card gets lost
			Mansus,		// Cannot save by design
			Greedy,		// Cannot save during Magnet grab (spec fix for #1253)
			WindowAnim,	// Cannot save during situation window open
			NumNonSaveableTypes
		};
        static private bool[] isInNonSaveableState = new bool[(int)NonSaveableType.NumNonSaveableTypes];
        private SituationController mansusSituation;
		//private Vector2 preMansusTabletopPos; // Disabled cause it looks jerky -Martin

		public static bool IsInMansus()
		{
			return isInNonSaveableState[(int)NonSaveableType.Mansus];
		}

		private float housekeepingTimer = 0.0f;	// Now a float so that we can time autosaves independent of Heart.Beat - CP
		private float AUTOSAVE_INTERVAL = 300.0f;
		private static float gridSnapSize = 0.0f;
        private List<string> currentDoomTokens=new List<string>();

		public void ForceAutosave()	// Useful for forcing autosave to happen at tricky moments for debugging - CP
		{
			housekeepingTimer = AUTOSAVE_INTERVAL;
		}

		public bool IsPaused()
		{
			return _heart.IsPaused;
		}

        public void Update()
		{
			//
			// Game is structured to minimise Update processing, so keep this lean - CP
			// But some things do have to be updated outside the main gameplay Heart.Beat
			//
            _hotkeyWatcher.WatchForGameplayHotkeys();
            _cardAnimationController.CheckForCardAnimations();

			if (_heart.IsPaused)
			{
				_heart.AdvanceTime( 0.0f );		// If the game is now calling Heart.Beat, we still need to update cosmetic stuff like Decay timers
			}

			// Failsafe to ensure that NonSaveableType.Drag never gets left on due to unusual exits from drag state - CP
			if (DraggableToken.itemBeingDragged == null)
				TabletopManager.RequestNonSaveableState( TabletopManager.NonSaveableType.Drag, false );

			housekeepingTimer += Time.deltaTime;
			if (housekeepingTimer >= AUTOSAVE_INTERVAL && IsSafeToAutosave())	// Hold off autsave until it's safe, rather than waiting for the next autosave - CP
			{
			    if (SaveGame(true))
				{
					housekeepingTimer = 0.0f;	// Successful save
				}
				else
				{
					housekeepingTimer = AUTOSAVE_INTERVAL-5.0f;		// Failed save - try again in 5 secs
				}
			}
        }

      

        void Start()
		{
       	    #if UNITY_STANDALONE_OSX
            // Vsync doesn't seem to limit the FPS on the mac so well, so we set it to 0 and force a target framerate (setting it to 0 any other way doesn't work, has to be done in code, apparently in Start not Awake too) - FM
            QualitySettings.vSyncCount = 0;
      	    Application.targetFrameRate = 60;
            #else
            QualitySettings.vSyncCount = 1;	// Force VSync on in case user has tried to disable it. No benefit, just burns CPU - CP
            #endif
            _situationBuilder = new SituationBuilder(tableLevelTransform, windowLevelTransform, _heart);
            NoonUtility.Log("Setting up services",10);
            //register everything used gamewide
            SetupServices(_situationBuilder, _tabletop);

            NoonUtility.Log("Initialising token containers", 10);
            // This ensures that we have an ElementStackManager in Limbo & Tabletop
            InitializeTokenContainers();

            NoonUtility.Log("Initialising subcontrollers", 10);

            //we hand off board functions to individual controllers
            InitialiseSubControllers(
                _speedController, 
                _hotkeyWatcher, 
                _cardAnimationController, 
                _mapController, 
                _endGameAnimController, 
                _notifier,
                _optionsPanel
                );

            InitialiseListeners();

            // Make sure dragging is reenabled
            DraggableToken.draggingEnabled = true;

            

            BeginGame(_situationBuilder);
        }

        /// <summary>
        /// if a game exists, load it; otherwise, create a fresh state and setup
        /// </summary>
        private void BeginGame(SituationBuilder builder)
		{
            //CHECK LEGACY POPULATED FOR CHARACTERS
            //this is all a bit post facto and could do with being tidied up
            //BUT now that legacies are saved in character data, it should only be relevant for old prelaunch saves.
			bool shouldStartPaused = false;
            NoonUtility.Log("Checking chosen legacy", 10);
            var chosenLegacy = CrossSceneState.GetChosenLegacy();
            if (chosenLegacy == null)
            {
                NoonUtility.Log("No initial Legacy specified",VerbosityLevel.Trivia);
                chosenLegacy = Registry.Retrieve<ICompendium>().GetAllLegacies().First();
                CrossSceneState.SetChosenLegacy(chosenLegacy);
                Registry.Retrieve<Character>() .ActiveLegacy = chosenLegacy;
            }

            if (CrossSceneState.GameState == GameState.Restarting)
            {
                NoonUtility.Log("Restarting game", 11);
                CrossSceneState.RestartingGame();
                BeginNewGame(builder);
            }
            else
            {
                NoonUtility.Log("Checking if save game exists", 10);
                var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());

                if (saveGameManager.DoesGameSaveExist() && saveGameManager.IsSavedGameActive())
                {
                    NoonUtility.Log("Loading game", 10);
                    LoadGame();
					shouldStartPaused = true;
                }
                else
                {
                    NoonUtility.Log("Beginning new game", 10);
                    BeginNewGame(builder);
                }
            }
			_heart.StartBeatingWithDefaultValue();						// Init heartbeat duration...
			_speedController.SetPausedState(shouldStartPaused, false);	// ...but (optionally) pause game while the player gets their bearings.
        }

        private void BeginNewGame(SituationBuilder builder)
        {
            SetupNewBoard(builder);
            var populatedCharacter =
                Registry.Retrieve<Character>(); //should just have been set above, but let's keep this clean
            Registry.Retrieve<ICompendium>().SupplyLevers(populatedCharacter);
            CrossSceneState.RestartedGame();
        }

        private void InitialiseSubControllers(SpeedController speedController,
                                              HotkeyWatcher hotkeyWatcher,
                                              CardAnimationController cardAnimationController,
                                              MapController mapController,
                                              EndGameAnimController endGameAnimController,
                                              Notifier notifier,
                                              OptionsPanel optionsPanel) {

            speedController.Initialise(_heart);
            hotkeyWatcher.Initialise(_speedController, debugTools, _optionsPanel);
            cardAnimationController.Initialise(_tabletop.GetElementStacksManager());
            mapController.Initialise(mapTokenContainer, mapBackground, mapAnimation);
            endGameAnimController.Initialise();
            notifier.Initialise();
            optionsPanel.InitPreferences(_speedController,true);
        }

        private void InitialiseListeners() {
            // Init Listeners to pre-existing DisplayHere Objects
            DraggableToken.onChangeDragState += HandleDragStateChanged;
            mapBackground.onDropped += HandleOnMapBackgroundDropped;
        }

        private void OnDestroy() {
            // Static event so make sure to de-init once this object is destroyed
            DraggableToken.onChangeDragState -= HandleDragStateChanged;
            mapBackground.onDropped -= HandleOnMapBackgroundDropped;
        }

        void InitializeTokenContainers() {
            _tabletop.Initialise();
            Limbo.Initialise();
            mapTokenContainer.Initialise();
        }

        private void SetupServices(SituationBuilder builder, TabletopTokenContainer container) {
            var registry = new Registry();
            var compendium = new Compendium();
            var contentImporter = new ContentImporter();
            contentImporter.PopulateCompendium(compendium);

            Character character;
            if (CrossSceneState.GetChosenLegacy() != null)
                character = new Character(CrossSceneState.GetChosenLegacy(), CrossSceneState.GetDefunctCharacter());
            else
                character = new Character(compendium.GetAllLegacies().First());


            var choreographer = new Choreographer(container, builder, tableLevelTransform, windowLevelTransform);
            var chronicler = new Chronicler(character,compendium);

            var situationsCatalogue = new SituationsCatalogue();
            var elementStacksCatalogue = new StackManagersCatalogue();

            //ensure we get updates about stack changes
            _elementOverview.Initialise(elementStacksCatalogue);

            var metaInfo=new MetaInfo(NoonUtility.VersionNumber);
            if(CrossSceneState.GetMetaInfo()==null)
            {
                          //This can happen if we start running the scene in the editor, so it hasn't been set in menu screen
                NoonUtility.Log("Setting meta info in CrossSceneState in Tabletop scene - it hadn't already been set",10);
                CrossSceneState.SetMetaInfo(metaInfo);
            }

            var draggableHolder = new DraggableHolder(draggableHolderRectTransform);

            var storeClientProvider=new StorefrontServicesProvider();
            storeClientProvider.InitialiseForStorefrontClientType(StoreClient.Steam);
            storeClientProvider.InitialiseForStorefrontClientType(StoreClient.Gog);

            registry.Register<ICompendium>(compendium);
            registry.Register<IDraggableHolder>(draggableHolder);
            registry.Register<IDice>(new Dice(debugTools));
            registry.Register<TabletopManager>(this);
            registry.Register<SituationBuilder>(builder);
            registry.Register<INotifier>(_notifier);
            registry.Register<Character>(character);
            registry.Register<Choreographer>(choreographer);
            registry.Register<Chronicler>(chronicler);
            registry.Register<MapController>(_mapController);
            registry.Register<Limbo>(Limbo);
            registry.Register<SituationsCatalogue>(situationsCatalogue);
            registry.Register<StackManagersCatalogue>(elementStacksCatalogue);
            registry.Register<MetaInfo>(metaInfo);
            registry.Register<StorefrontServicesProvider>(storeClientProvider);


        }



        #region -- Build / Reset -------------------------------

        public void SetupNewBoard(SituationBuilder builder) {


            builder.CreateInitialTokensOnTabletop();
            Character _character = Registry.Retrieve<Character>();
            if(_character.ActiveLegacy==null)
                throw new ApplicationException("Trying to set up a new board for a character with no chosen legacy. Even fresh characters should have a legacy when created, but this code has always been hinky.");
            ProvisionStartingElements(_character.ActiveLegacy, Registry.Retrieve<Choreographer>());
            SetStartingCharacterInfo(_character.ActiveLegacy);
            StatusBar.UpdateCharacterDetailsView(Registry.Retrieve<Character>());

            DealStartingDecks();

            _notifier.ShowNotificationWindow(_character.ActiveLegacy.Label, _character.ActiveLegacy.StartDescription);
        }

        private void SetStartingCharacterInfo(Legacy chosenLegacy)
		{
            Character newCharacter = Registry.Retrieve<Character>();
            newCharacter.Name = LanguageTable.Get("UI_CLICK_TO_NAME");
           // Registry.Retrieve<Chronicler>().CharacterNameChanged(NoonConstants.DEFAULT_CHARACTER_NAME);//so we never see a 'click to rename' in future history
            newCharacter.Profession = chosenLegacy.Label;   
        }

        private void DealStartingDecks() {
            IGameEntityStorage character = Registry.Retrieve<Character>();
            var compendium = Registry.Retrieve<ICompendium>();
            foreach (var ds in compendium.GetAllDeckSpecs()) {
                IDeckInstance di = new DeckInstance(ds);
                character.DeckInstances.Add(di);
                di.Reset();
            }
        }

        public void ProvisionStartingElements(Legacy chosenLegacy, Choreographer choreographer) {
            AspectsDictionary startingElements = new AspectsDictionary();
            startingElements.CombineAspects(chosenLegacy.Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.

            foreach (var e in startingElements) {
                ElementStackToken token = _tabletop.ProvisionElementStack(e.Key, e.Value, Source.Existing()) as ElementStackToken;
                choreographer.ArrangeTokenOnTable(token, new Context(Context.ActionSource.Loading));
            }
        }

        public void ClearGameState(Heart h, IGameEntityStorage s, TabletopTokenContainer tc) {
            h.Clear();
            s.DeckInstances = new List<IDeckInstance>();

            foreach (var sc in Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations())
                sc.Retire();

            foreach (var element in tc.GetElementStacksManager().GetStacks())
                element.Retire(true); //looks daft but pretty on reset
        }

        public void RestartGame() {
            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());
            CrossSceneState.RestartingGame();
            

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void EndGame(Ending ending, SituationController endingSituation) {
            var ls = new LegacySelector(Registry.Retrieve<ICompendium>());
            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());

            var character = Registry.Retrieve<Character>();
            var chronicler = Registry.Retrieve<Chronicler>();
            
            chronicler.ChronicleGameEnd(Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations(), Registry.Retrieve<StackManagersCatalogue>().GetRegisteredStackManagers(),ending);


            CrossSceneState.SetCurrentEnding(ending);
            CrossSceneState.SetDefunctCharacter(character);
            CrossSceneState.SetAvailableLegacies(ls.DetermineLegacies(ending, null));

            //#if !DEBUG
            saveGameManager.SaveInactiveGame(null);
            //#endif

            string animName;

            if (string.IsNullOrEmpty(ending.Anim))
                animName = "DramaticLight";
            else
                animName = ending.Anim;

            // TODO: Get effect name from ending?
            _endGameAnimController.TriggerEnd((SituationToken)endingSituation.situationToken, animName);
        }

#endregion

#region -- Load / Save GameState -------------------------------

        public void LoadGame() {
            ICompendium compendium = Registry.Retrieve<ICompendium>();
            IGameEntityStorage storage = Registry.Retrieve<Character>();

            _speedController.SetPausedState(true, false);
            var saveGameManager = new GameSaveManager(new GameDataImporter(compendium), new GameDataExporter());
            //try
            //{
            var htSave = saveGameManager.RetrieveHashedSaveFromFile();
            ClearGameState(_heart, storage, _tabletop);
            saveGameManager.ImportHashedSaveToState(_tabletop, storage, htSave);

            //my early Jenga code: the gift that keeps on giving. Here, we cater for cases where a gently borked saved game just imported a null ActiveLegacy
            /////
            if (storage.ActiveLegacy == null)
                storage.ActiveLegacy = compendium.GetAllLegacies().First();
            /////
            CrossSceneState.SetChosenLegacy(storage.ActiveLegacy); // man this is spaghetti. 'Don't forget to update the global variable after you imported it into a different object'. MY BAD. - AK
            StatusBar.UpdateCharacterDetailsView(storage);

			// Reopen any windows that were open at time of saving. I think there can only be one, but checking all for robustness - CP
			var allSituationControllers = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();
            foreach (var s in allSituationControllers)
			{
				if (s.IsOpen)
				{
					Vector3 tgtPos = s.RestoreWindowPosition;
	                s.OpenWindow( tgtPos );
				}
			}

            _notifier.ShowNotificationWindow( LanguageTable.Get("UI_LOADEDTITLE"), LanguageTable.Get("UI_LOADEDDESC"));

            //}
            //catch (Exception e)
            //{
            //    _notifier.ShowNotificationWindow("Couldn't load game - ", e.Message);
            //}
            _speedController.SetPausedState(true, false);
        }

        public bool SaveGame(bool withNotification)
		{
            if (!IsSafeToAutosave())
                return false;
			
			bool success = true;	// Assume everything will be OK to begin with...

			// Check state so that autosave behaves correctly if called while paused - CP
			bool wasBeating = false;
			if (!_heart.IsPaused)
			{
		        _heart.StopBeating();
				wasBeating = true;
			}
			/*
            //Close all windows and dump tokens to desktop before saving.
            //We don't want or need to track half-started situations.
            var allSituationControllers = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();
            foreach (var s in allSituationControllers)
                s.CloseWindow();
			*/

            // try
            //  {
            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());
            success = saveGameManager.SaveActiveGame(_tabletop, Registry.Retrieve<Character>());
			if (success)
			{
				if (withNotification)
				{
					//_notifier.ShowNotificationWindow("SAVED THE GAME", "BUT NOT THE WORLD");
					_autosaveNotifier.SetDuration( 3.0f );
					_autosaveNotifier.Show();
				}
			}
            //}
            //catch (Exception e)
            //{
            //      _notifier.ShowNotificationWindow("Couldn't save game - ", e.Message); ;
            // }

			if (wasBeating)
			{
	            _heart.ResumeBeating();
			}

			if (GameSaveManager.saveErrorWarningTriggered)	// Do a full pause after resuming heartbeat (to update UI, SFX, etc)
			{
				bool pauseStateWhenErrorRequested = GetPausedState();
				if (!pauseStateWhenErrorRequested)			// only pause if we need to (since it triggers sfx)
					SetPausedState(true);
				GameSaveManager.saveErrorWarningTriggered = false;	// Clear after we've used it
			}

			return success;
        }

#endregion

#region -- Greedy Grabbing -------------------------------

        public HashSet<TokenAndSlot> FillTheseSlotsWithFreeStacks(HashSet<TokenAndSlot> slotsToFill) {
            var unprocessedSlots = new HashSet<TokenAndSlot>();
            var choreo = Registry.Retrieve<Choreographer>();
            SituationController sit;

            foreach (var tokenSlotPair in slotsToFill) {
                if (NeedToFillSlot(tokenSlotPair) == false) 
                    continue; // Skip it, we don't need to fill it

                var stack = FindStackForSlotSpecificationOnTabletop(tokenSlotPair.RecipeSlot.GoverningSlotSpecification) as ElementStackToken;

                if (stack != null) {
                    stack.SplitAllButNCardsToNewStack(1, new Context(Context.ActionSource.GreedySlot));
                    choreo.MoveElementToSituationSlot(stack, tokenSlotPair, choreo.ElementGreedyAnimDone);
                    continue; // we found a stack, we're done here
                }

                stack = FindStackForSlotSpecificationInSituations(tokenSlotPair.RecipeSlot.GoverningSlotSpecification, out sit) as ElementStackToken;

                if (stack != null) {
                    stack.SplitAllButNCardsToNewStack(1, new Context(Context.ActionSource.GreedySlot));
                    choreo.PrepareElementForGreedyAnim(stack, sit.situationToken as SituationToken); // this reparents the card so it can animate properly
                    choreo.MoveElementToSituationSlot(stack, tokenSlotPair, choreo.ElementGreedyAnimDone);
                    continue; // we found a stack, we're done here
                }
                
                unprocessedSlots.Add(tokenSlotPair);
            }

            return unprocessedSlots;
        }

        private bool NeedToFillSlot(TokenAndSlot tokenSlotPair) {
            if (tokenSlotPair.Token.Equals(null))
                return false; // It has been destroyed
            if (tokenSlotPair.Token.Defunct)
                return false;
            if (!tokenSlotPair.Token.SituationController.IsOngoing)
                return false;
            if (tokenSlotPair.RecipeSlot.Equals(null))
                return false; // It has been destroyed
            if (tokenSlotPair.RecipeSlot.Defunct)
                return false;
            if (tokenSlotPair.RecipeSlot.IsBeingAnimated)
                return false; // We're animating something into the slot.
            if (tokenSlotPair.RecipeSlot.GetElementStackInSlot() != null)
                return false; // It is already filled
            if (tokenSlotPair.RecipeSlot.GoverningSlotSpecification==null || !tokenSlotPair.RecipeSlot.GoverningSlotSpecification.Greedy)
                return false; //it's not greedy any more; sometimes if we have a recipe with a greedy slot followed by a recipe with a non-greedy slot, the behaviour carries over for the moment the recipe changes

            return true;
        }

        private IElementStack FindStackForSlotSpecificationOnTabletop(SlotSpecification slotSpec) {

            var rnd = new Random();
            var stacks = _tabletop.GetElementStacksManager().GetStacks().OrderBy(x=>rnd.Next());

            foreach (var stack in stacks)
                if (CanPullCardToGreedySlot(stack as ElementStackToken, slotSpec))
                    return stack;
    
            return null;
        }

        private bool CanPullCardToGreedySlot(ElementStackToken stack, SlotSpecification slotSpec)
        {
            if (slotSpec == null)
                return false; //We were seeing NullReferenceExceptions in the Unity analytics from the bottom line; stack is referenced okay so it shouldn't be stack, so probably a null slotspec is being specified somewhere

            if (stack == null) //..but just in case.
                return false;

            if (stack.Defunct)
                return false; // don't pull defunct cards
            else if (stack.IsBeingAnimated)
                return false; // don't pull animated cards
            else if (DraggableToken.itemBeingDragged == stack)
                return false; // don't pull cards being dragged

            return slotSpec.GetSlotMatchForAspects(stack.GetAspects()).MatchType == SlotMatchForAspectsType.Okay;
        }

        private IElementStack FindStackForSlotSpecificationInSituations(SlotSpecification slotSpec, out SituationController sit) {
            var rnd = new Random();

            // Nothing on the table? Look at the Situations.
            var situationControllers = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();

            // We grab output first
            foreach (var controller in situationControllers) {
                foreach (var stack in controller.GetOutputStacks().OrderBy(x=>rnd.Next())) {
                    if (CanPullCardToGreedySlot(stack as ElementStackToken, slotSpec)) {
                        sit = controller;
                        return stack;
                    }
                }
            }

            // Nothing? Then We grab starting
            foreach (var controller in situationControllers) {
                foreach (var stack in controller.GetStartingStacks()) {
                    if (CanPullCardToGreedySlot(stack as ElementStackToken, slotSpec)) {
                        sit = controller;
                        return stack;
                    }
                }
            }

            // Nothing? Then we grab ongoing
            foreach (var controller in situationControllers.OrderBy(x => rnd.Next())) {
                foreach (var slot in controller.GetOngoingSlots()) {
                    if (slot.IsGreedy)
                        continue; // Greedy? Don't grab.

                    var stack = slot.GetElementStackInSlot();

                    if (stack == null)
                        continue; // Empty? Nothing to grab either

                    if (CanPullCardToGreedySlot(stack as ElementStackToken, slotSpec)) {
                        sit = controller;
                        return stack;
                    }
                }
            }

            sit = null;
            return null;
        }

#endregion

        public void CloseAllSituationWindowsExcept(string exceptTokenId) {
            var situationControllers = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();

            foreach (var controller in situationControllers) {
                if (controller.GetTokenId() != exceptTokenId)
                    controller.CloseWindow();
            }
        }

        void HandleOnMapBackgroundDropped() {
            if (DraggableToken.itemBeingDragged != null) {

                DraggableToken.SetReturn(false, "dropped on the map background");
                DraggableToken.itemBeingDragged.DisplayAtTableLevel();
                mapTokenContainer.DisplayHere(DraggableToken.itemBeingDragged, new Context(Context.ActionSource.PlayerDrag));

                SoundManager.PlaySfx("CardDrop");
            }
        }

        public void DecayStacksOnTable(float interval) {
            var decayingStacks = _tabletop.GetElementStacksManager().GetStacks().Where(s => s.Decays);

            foreach (var d in decayingStacks)
                d.Decay(interval);
        }


        public void DecayStacksInResults(float interval)
        {
            var situationControllers = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();

            foreach (var s in situationControllers)
            {
                s.TryDecayContents(interval);
            }

            //foreach (var d in decayingStacks)
              //  d.Decay(interval);
        }

        private void HandleDragStateChanged(bool isDragging) {
            // not dragging a stack? then do nothing. _tabletop was destroyed (end of game?)
            if (_tabletop == null)
                return;

            var draggedElement = DraggableToken.itemBeingDragged as ElementStackToken;

            if (mapTokenContainer != null)
                mapTokenContainer.ShowDestinationsForStack(draggedElement, isDragging);
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

        public void SetPausedState(bool paused) {
            _speedController.SetPausedState(paused);
        }

		public bool GetPausedState() {
            return _speedController.GetPausedState();
        }

        void LockSpeedController(bool enabled) {
            _speedController.LockToPause(enabled);
        }

		public void SetAutosaveInterval( float minutes )
		{
			AUTOSAVE_INTERVAL = minutes * 60.0f;
		}

		public void SetGridSnapSize( float snapsize )
		{
			int snap = Mathf.RoundToInt( snapsize );
			switch (snap)
			{
			default:
			case 0:		gridSnapSize = 0.0f; break;
			case 1:		gridSnapSize = 1.0f; break;		// 1 card
			case 2:		gridSnapSize = 0.5f; break;		// ½ card
			case 3:		gridSnapSize = 0.25f; break;	// ¼ card
			}
		}

		public static float GetGridSnapSize()
		{
			return gridSnapSize;
		}

        public void ShowMansusMap(SituationController situation, Transform origin, PortalEffect effect) {
            CloseAllSituationWindowsExcept(null);

            DraggableToken.CancelDrag();
            LockSpeedController(true);
            RequestNonSaveableState( NonSaveableType.Mansus, true );

            SoundManager.PlaySfx("MansusEntry");
            // Play Mansus Music
            backgroundMusic.PlayMansusClip();

            // Build mansus cards and doors everything
            mansusSituation = situation; // so we can drop the card in the right place
            _mapController.SetupMap(effect);

            var chronicler = Registry.Retrieve<Chronicler>();
            chronicler.ChronicleMansusEntry(effect);

			//preMansusTabletopPos = tableScroll.content.anchoredPosition;

            // Do transition
            _tabletop.Show(false);
            _mapController.ShowMansusMap(origin, true);
        }

        public void ReturnFromMansus(Transform origin, ElementStackToken mansusCard) {
            DraggableToken.CancelDrag();
            LockSpeedController(false);
            FlushNonSaveableState();	// On return from Mansus we can't possibly be overlapping with any other non-autosave state so force a reset for safety - CP

            // Play Normal Music
            backgroundMusic.PlayRandomClip();

            // Cleanup mansus cards and doors everything
            _mapController.CleanupMap(mansusCard);

            // Do transition
            _tabletop.Show(true);
            _mapController.ShowMansusMap(origin, false);
            SoundManager.PlaySfx("MansusExit");

            // Put card into the original Situation Results
            mansusSituation.AddToResults(mansusCard, new Context(Context.ActionSource.PlayerDrag));
            mansusSituation.AddNote(new Notification(string.Empty, mansusCard.IlluminateLibrarian.PopMansusJournalEntry()));
            mansusSituation.OpenWindow();

            // insta setting back to last position before the mansus was transformed, but I don't like it. Feels jerky. - martin
			//tableScroll.content.anchoredPosition = preMansusTabletopPos;
            mansusSituation = null;
        }

        public void BeginNewSituation(SituationCreationCommand scc,List<IElementStack> withStacksInStorage) {
            Registry.Retrieve<Choreographer>().BeginNewSituation(scc,withStacksInStorage);
        }

        public void SignalImpendingDoom(ISituationAnchor situationToken) {
            if(!currentDoomTokens.Contains(situationToken.EntityId))
                currentDoomTokens.Add(situationToken.EntityId);
            backgroundMusic.PlayImpendingDoom();
        }


        public void NoMoreImpendingDoom(ISituationAnchor situationToken)
        {
            if (currentDoomTokens.Contains(situationToken.EntityId))
                currentDoomTokens.Remove(situationToken.EntityId);
            if(!currentDoomTokens.Any())
                backgroundMusic.NoMoreImpendingDoom();
        }

		public void HighlightAllStacksForSlotSpecificationOnTabletop(SlotSpecification slotSpec) {
			var stacks = FindAllStacksForSlotSpecificationOnTabletop(slotSpec);

			foreach (var stack in stacks) {
				ShowFXonToken("FX/CardPingEffect", stack.transform);
			}
		}

		private List<ElementStackToken> FindAllStacksForSlotSpecificationOnTabletop(SlotSpecification slotSpec) {
			var stackList = new List<ElementStackToken>();
			var stacks = _tabletop.GetElementStacksManager().GetStacks();
			ElementStackToken stackToken;

			foreach (var stack in stacks) {
				stackToken = stack as ElementStackToken;

				if (stackToken != null && CanPullCardToGreedySlot(stackToken, slotSpec))
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

#if UNITY_EDITOR
		private void OnGUI()
		{
			// Extra tools for debugging autosave.

			// Toggle to simulate bad save
			if (GUI.Button( new Rect(Screen.width * 0.5f - 300f, 10f, 180f, 20f), "Simulate bad save: " + (GameSaveManager.simulateBrokenSave?"ON":"off") ))
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
		}
#endif
	}

}
