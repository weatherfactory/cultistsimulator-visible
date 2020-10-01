#pragma warning disable 0649

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
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts.UI;
using Assets.TabletopUi.UI;
using Noon;
using TabletopUi.Scripts.Elements;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace Assets.CS.TabletopUI {
    public class TabletopManager : MonoBehaviour, IStacksChangeSubscriber,ISettingSubscriber
    {


        [SerializeField] private EndGameAnimController _endGameAnimController;

        [Header("Tabletop")] [SerializeField] public TabletopTokenContainer _tabletop;
        [SerializeField] TabletopBackground tabletopBackground;

        [SerializeField] private HighlightLocationsController _highlightLocationsController;

        
        [Header("Detail Windows")] [SerializeField]
        private AspectDetailsWindow aspectDetailsWindow;

        [SerializeField] private TokenDetailsWindow tokenDetailsWindow;
        [SerializeField] private CardHoverDetail cardHoverDetail;

        [Header("Mansus Map")] [SerializeField]
        private MapController _mapController;

        [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("mapContainsTokens")]
        public MapTokenContainer mapTokenContainer;

        [SerializeField] TabletopBackground mapBackground;
        [SerializeField] MapAnimation mapAnimation;

        [Header("Drag & Window")] [SerializeField]
        private RectTransform draggableHolderRectTransform;

        [SerializeField] Transform tableLevelTransform;
        [SerializeField] Transform windowLevelTransform;
        [SerializeField] private ScrollRect tableScroll;
        [SerializeField] public GameObject _dropZoneTemplate;


        [Header("Options Bar & Notes")] [SerializeField]
        private StatusBar StatusBar;

        [SerializeField] private BackgroundMusic backgroundMusic;

        [SerializeField] private Notifier _notifier;
        [SerializeField] private AutosaveWindow _autosaveNotifier;
        [SerializeField] private ElementOverview _elementOverview;

        private SituationBuilder _situationBuilder;






        private bool disabled;
        private bool _initialised;

        // Internal cache - if ENABLE_ASPECT_CACHING disabled, if still uses these but recalcs every frame
        [NonSerialized] public bool _enableAspectCaching = true;

        private AspectsDictionary _tabletopAspects = null;
        private AspectsDictionary _allAspectsExtant = null;
        private bool _tabletopAspectsDirty = true;
        private bool _allAspectsExtantDirty = true;

        public void NotifyAspectsDirty()
        {
            _tabletopAspectsDirty = true;
        }

        public enum NonSaveableType
        {
            Drag, // Cannot save because held card gets lost
            Mansus, // Cannot save by design
            Greedy, // Cannot save during Magnet grab (spec fix for #1253)
            WindowAnim, // Cannot save during situation window open
            NumNonSaveableTypes
        };

        static private bool[] isInNonSaveableState = new bool[(int) NonSaveableType.NumNonSaveableTypes];

        private SituationController mansusSituation;
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
        private static bool stickyDragMode = false;
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

            // Failsafe to ensure that NonSaveableType.Drag never gets left on due to unusual exits from drag state - CP
            if (DraggableToken.itemBeingDragged == null)
                TabletopManager.RequestNonSaveableState(TabletopManager.NonSaveableType.Drag, false);

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
            

            
            _situationBuilder = new SituationBuilder(tableLevelTransform, windowLevelTransform);

            //register everything used gamewide
            SetupServices(registry, _situationBuilder, _tabletop);

            InitializeTokenContainers();

            //we hand off board functions to individual controllers
            InitialiseSubControllers(
                _mapController,
                _endGameAnimController
            );

            InitialiseListeners();

            // Make sure dragging is reenabled
            DraggableToken.draggingEnabled = true;

            _initialised = true;


            if (Registry.Get<StageHand>().SourceForGameState == SourceForGameState.NewGame)
            {
                BeginNewGame(_situationBuilder);
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


            var saveGameManager =
                new GameSaveManager(new GameDataImporter(Registry.Get<ICompendium>()), new GameDataExporter());
            bool isSaveCorrupted = false;
            bool shouldContinueGame;
            try
            {
                shouldContinueGame = saveGameManager.DoesGameSaveExist() && saveGameManager.IsSavedGameActive(source);
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



    private void BeginNewGame(SituationBuilder builder)
        {
            SetupNewBoard(builder);
            var populatedCharacter =
                Registry.Get<Character>(); //should just have been set above, but let's keep this clean
            populatedCharacter.Reset(populatedCharacter.ActiveLegacy,null);
            Registry.Get<ICompendium>().SupplyLevers(populatedCharacter);
     Registry.Get<StageHand>().ClearRestartingGameFlag();
        }

        private void InitialiseSubControllers(MapController mapController,
                                              EndGameAnimController endGameAnimController) {

            mapController.Initialise(mapTokenContainer, mapBackground, mapAnimation);
            endGameAnimController.Initialise();
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
            mapTokenContainer.Initialise();
        }



        private void SetupServices(Registry registry,SituationBuilder builder, TabletopTokenContainer container)
        {

            registry.Register(builder);


            var choreographer = new Choreographer(container, builder, tableLevelTransform, windowLevelTransform);
            registry.Register(choreographer);

            var situationsCatalogue = new SituationsCatalogue();
            registry.Register(situationsCatalogue);

            Registry.Get<StackManagersCatalogue>().Subscribe(this);


            var metaInfo = new MetaInfo(new VersionNumber(Application.version));
            registry.Register(metaInfo);

            var draggableHolder = new DraggableHolder(draggableHolderRectTransform);
            registry.Register<IDraggableHolder>(draggableHolder);


            registry.Register<INotifier>(_notifier);

            
            
			registry.Register<HighlightLocationsController>(_highlightLocationsController);
            _highlightLocationsController.Initialise(Registry.Get<StackManagersCatalogue>());


            //element overview needs to be initialised with
            // - legacy - in case we're displaying unusual info
            // stacks catalogue - so it can subscribe for notifications re changes
            Legacy legacy = Registry.Get<Character>().ActiveLegacy;

            //this shouldn't happen, but here's a guard clause in case it does.
            if(legacy==null)
                Registry.Get<StageHand>().EndingScreen();

            _elementOverview.Initialise(legacy,  Registry.Get<ICompendium>());
            Registry.Get<StackManagersCatalogue>().Subscribe(_elementOverview);
            tabletopBackground.ShowTabletopFor(legacy);

  

        }






        public void SetupNewBoard(SituationBuilder builder) {


     
            Character _character = Registry.Get<Character>();
            if(_character.ActiveLegacy==null)
                throw new ApplicationException("Trying to set up a new board for a character with no chosen legacy. Even fresh characters should have a legacy when created, but this code has always been hinky.");

            builder.CreateInitialTokensOnTabletop(_character.ActiveLegacy);

            ProvisionStartingElements(_character.ActiveLegacy, Registry.Get<Choreographer>());
            SetStartingCharacterInfo(_character.ActiveLegacy);
            StatusBar.UpdateCharacterDetailsView(Registry.Get<Character>());

            DealStartingDecks();

            _notifier.ShowNotificationWindow(_character.ActiveLegacy.Label, _character.ActiveLegacy.StartDescription);
        }

        private void SetStartingCharacterInfo(Legacy chosenLegacy)
		{
            Character newCharacter = Registry.Get<Character>();
            newCharacter.Name = Registry.Get<ILocStringProvider>().Get("UI_CLICK_TO_NAME");
           // Registry.Retrieve<Chronicler>().CharacterNameChanged(NoonConstants.DEFAULT_CHARACTER_NAME);//so we never see a 'click to rename' in future history
            newCharacter.Profession = chosenLegacy.Label;
        }

        private void DealStartingDecks() {
            Character character = Registry.Get<Character>();
            var compendium = Registry.Get<ICompendium>();
            foreach (var ds in compendium.GetEntitiesAsList<DeckSpec>()) {
                IDeckInstance di = new DeckInstance(ds);
                character.DeckInstances.Add(di);
                di.Reset();
            }
        }

        public void ProvisionStartingElements(Legacy chosenLegacy, Choreographer choreographer) {
            AspectsDictionary startingElements = new AspectsDictionary();
            startingElements.CombineAspects(chosenLegacy.Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.

            foreach (var e in startingElements)
            {
                var context = new Context(Context.ActionSource.Loading);

                ElementStackToken token = _tabletop.ProvisionElementStack(e.Key, e.Value, Source.Existing(),context) as ElementStackToken;
                choreographer.ArrangeTokenOnTable(token, context);
            }
        }


        public void PurgeElement(string elementId, int maxToPurge)
        {
            var compendium = Registry.Get<ICompendium>();

            Element purgedElement = compendium.GetEntityById<Element>(elementId);
            //I don't think MaxToPurge is being usefully decremented here - should return int

           _tabletop.GetElementStacksManager().PurgeElement(purgedElement, maxToPurge);

           var situationsCatalogue = Registry.Get<SituationsCatalogue>();
           foreach (var s in situationsCatalogue.GetRegisteredSituations())
           {

               if (s.SituationClock.State == SituationState.Unstarted)
               {
                   var slotsToTryPurge = new List<RecipeSlot>(s.situationWindow.GetStartingSlots());

                   slotsToTryPurge.Reverse();
                   foreach (var slot in slotsToTryPurge)
                       slot.TryPurgeElement(purgedElement, maxToPurge);
               }
               //If the situation has finished, purge any matching elements in the results.
                else if (s.SituationClock.State==SituationState.Complete)
                { 
                   s.situationWindow.GetResultsStacksManager()
                       .PurgeElement(purgedElement, maxToPurge);

                }
                else
                 {
                   //if the situation is still ongoing, any elements actually inside it are protected. However, elements in the slot are not protected.
                   s.situationWindow.GetOngoingSlots().FirstOrDefault()
                       ?.TryPurgeElement(purgedElement, maxToPurge);
                 }
           }
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
                    if (s.GetTokenId().StartsWith(wildcardToDelete))
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
                    if (s.GetTokenId() == toHaltId.Trim())
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
                    if (s.GetTokenId().StartsWith(wildcardToDelete))
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
                 if (s.GetTokenId() == toDeleteId.Trim())
                        {
                            s.Retire();
                            i++;
                        }
                        if (i >= maxToDelete)
                            break;
                    }
                }
        }


        public async void EndGame(Ending ending, SituationController endingSituation)
		{
			NoonUtility.Log("TabletopManager.EndGame()");

            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Get<ICompendium>()), new GameDataExporter());

            var character = Registry.Get<Character>();
            var chronicler = Registry.Get<Chronicler>();

            chronicler.ChronicleGameEnd(Registry.Get<SituationsCatalogue>().GetRegisteredSituations(), Registry.Get<StackManagersCatalogue>().GetRegisteredStackManagers(),ending);
            character.Reset(null,ending);
            


            var saveTask = saveGameManager.SaveActiveGameAsync(new InactiveTableSaveState(), Registry.Get<Character>(),SourceForGameState.DefaultSave);
            var result = await saveTask;

        

            
            _endGameAnimController.TriggerEnd((SituationToken)endingSituation.situationToken, ending);
        }





        public void LoadGame(SourceForGameState gameStateSource) {
            ICompendium compendium = Registry.Get<ICompendium>();


            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs
                {ControlPriorityLevel = 1, GameSpeed = GameSpeed.Paused, WithSFX = false});
            Registry.Get<LocalNexus>().UILookAtMeEvent.Invoke(typeof(SpeedControlUI));
            var saveGameManager = new GameSaveManager(new GameDataImporter(compendium), new GameDataExporter());
            try
            {
	            //var htSave = saveGameManager.RetrieveHashedSaveFromFile(index);
	        //    ClearGameState(_heart, character, _tabletop);
            var registry=new Registry();
           registry.Register(saveGameManager.LoadCharacterState(gameStateSource));
            saveGameManager.LoadTabletopState(_tabletop,gameStateSource);
                //saveGameManager.ImportHashedSaveToState(_tabletop, null, htSave);

                StatusBar.UpdateCharacterDetailsView(Registry.Get<Character>());

				// Reopen any windows that were open at time of saving. I think there can only be one, but checking all for robustness - CP
				var allSituationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();
	            foreach (var s in allSituationControllers)
				{
					if (s.IsOpen)
					{
						Vector3 tgtPos = s.RestoreWindowPosition;
		                s.OpenWindow( tgtPos );
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

            Registry.Get<StackManagersCatalogue>().Reset();
            Registry.Get<StackManagersCatalogue>().Subscribe(_elementOverview);
            tabletopBackground.ShowTabletopFor(activeLegacy);

        }

        public async Task<bool> SaveGameAsync(bool withNotification, SourceForGameState source)
		{

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
	            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Get<ICompendium>()), new GameDataExporter());

                ITableSaveState tableSaveState=new TableSaveState(_tabletop.GetElementStacksManager().GetStacks(), Registry.Get<SituationsCatalogue>().GetRegisteredSituations());
                 var   saveTask = saveGameManager.SaveActiveGameAsync(tableSaveState,  Registry.Get<Character>(), source);
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


        #region -- Greedy Grabbing -------------------------------

        public HashSet<TokenAndSlot> FillTheseSlotsWithFreeStacks(HashSet<TokenAndSlot> slotsToFill) {
            var unprocessedSlots = new HashSet<TokenAndSlot>();
            var choreo = Registry.Get<Choreographer>();
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

        private ElementStackToken FindStackForSlotSpecificationOnTabletop(SlotSpecification slotSpec) {

            var rnd = new Random();
            var stacks = _tabletop.GetElementStacksManager().GetStacks().OrderBy(x=>rnd.Next());

            foreach (var stack in stacks)
                if (CanPullCardToGreedySlot(stack as ElementStackToken, slotSpec))
                {
#pragma warning disable CS0253 // Possible unintended reference comparison; right hand side needs cast
                    if (DraggableToken.itemBeingDragged == stack)
#pragma warning restore CS0253 // Possible unintended reference comparison; right hand side needs cast
                    {
                        DraggableToken.SetReturn(false,"Drag aborted by greedy slot"); DraggableToken.itemBeingDragged = null;
                    }
                
                    return stack;
                }

            
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


             var allowExploits = Registry.Get<Config>().GetConfigValueAsInt(NoonConstants.BIRDWORMSLIDER);
                if (allowExploits!=null || allowExploits > 0)
                {
                    Debug.Log("exploits on");
                    if (DraggableToken.itemBeingDragged == stack)
                        return false; // don't pull cards being dragged if Worm is set On}
                }
            

            return slotSpec.GetSlotMatchForAspects(stack.GetAspects()).MatchType == SlotMatchForAspectsType.Okay;
        }

        private ElementStackToken FindStackForSlotSpecificationInSituations(SlotSpecification slotSpec, out SituationController sit) {
            var rnd = new Random();

            // Nothing on the table? Look at the Situations.
            var situationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

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

public ElementStacksManager GetTabletopStacksManager()
{
    return _tabletop.GetElementStacksManager();
}

		public void CloseAllDetailsWindows()
		{
			if (aspectDetailsWindow != null)
				aspectDetailsWindow.Hide();
			if (tokenDetailsWindow != null)
				tokenDetailsWindow.Hide();
		}

        public void CloseAllSituationWindowsExcept(string exceptTokenId) {
            var situationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

            foreach (var controller in situationControllers) {
                if (controller.GetTokenId() != exceptTokenId)
                    controller.CloseWindow();
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
	        if (elementId == null || elementId == "dropzone")
	        {
		        cardHoverDetail.Hide();
		        return;
	        }
	        cardHoverDetail.Populate(elementId, quantity);
	        cardHoverDetail.Show();
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
            var situationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();

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

		public static void SetStickyDrag( bool on )
		{
			stickyDragMode = on;
		}

		public static bool GetStickyDrag()
		{
			return stickyDragMode;
		}

        public void ShowMansusMap(SituationController situation, Transform origin, PortalEffect effect) {
            CloseAllSituationWindowsExcept(null);

            DraggableToken.CancelDrag();

            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs{ControlPriorityLevel = 3,GameSpeed=GameSpeed.Paused,WithSFX =false});
            RequestNonSaveableState( NonSaveableType.Mansus, true );

            SoundManager.PlaySfx("MansusEntry");
            // Play Mansus Music
            backgroundMusic.PlayMansusClip();

            // Build mansus cards and doors everything
            mansusSituation = situation; // so we can drop the card in the right place
            _mapController.SetupMap(effect);

            var chronicler = Registry.Get<Chronicler>();
            chronicler.ChronicleMansusEntry(effect);

			//preMansusTabletopPos = tableScroll.content.anchoredPosition;

            // Do transition
            _tabletop.Show(false);
            _mapController.ShowMansusMap(origin, true);
        }

        public void ReturnFromMansus(Transform origin, ElementStackToken mansusCard)
		{
            DraggableToken.CancelDrag();

            FlushNonSaveableState();	// On return from Mansus we can't possibly be overlapping with any other non-autosave state so force a reset for safety - CP

            // Play Normal Music
            backgroundMusic.PlayRandomClip();

            // Cleanup mansus cards and doors everything
            _mapController.CleanupMap(mansusCard);

            // Do transition
            _tabletop.Show(true);
            _mapController.ShowMansusMap(origin, false);
            SoundManager.PlaySfx("MansusExit");

            // Pause the game with a flashing notification
            Registry.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs { ControlPriorityLevel =3 , GameSpeed = GameSpeed.Paused, WithSFX = false});

           Registry.Get<LocalNexus>().UILookAtMeEvent.Invoke(typeof(SpeedControlUI));

            // Put card into the original Situation Results
			mansusCard.lastTablePos = null;	// Flush last known desktop position so it's treated as brand new
            mansusSituation.AddToResults(mansusCard, new Context(Context.ActionSource.PlayerDrag));
            mansusSituation.AddNote(new Notification(string.Empty, mansusCard.IlluminateLibrarian.PopMansusJournalEntry()));
            mansusSituation.OpenWindow();

            // insta setting back to last position before the mansus was transformed, but I don't like it. Feels jerky. - martin
			//tableScroll.content.anchoredPosition = preMansusTabletopPos;
            mansusSituation = null;
        }

        public void BeginNewSituation(SituationCreationCommand scc,List<ElementStackToken> withStacksInStorage) {
            Registry.Get<Choreographer>().BeginNewSituation(scc,withStacksInStorage);
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

		static private float cardPingLastTriggered = 0.0f;

		public void HighlightAllStacksForSlotSpecificationOnTabletop(SlotSpecification slotSpec)
		{
			float time = Time.realtimeSinceStartup;
			if (time > cardPingLastTriggered + 1.0f)	// Don't want to trigger these within a second of the last trigger, otherwise they stack up too much
			{
				cardPingLastTriggered = time;

				var stacks = FindAllStacksForSlotSpecificationOnTabletop(slotSpec);

				foreach (var stack in stacks)
				{
					ShowFXonToken("FX/CardPingEffect", stack.transform);
				}
			}
		}

        public AspectsInContext GetAspectsInContext(IAspectsDictionary aspectsInSituation)
        {
			if (!_enableAspectCaching)
			{
				_tabletopAspectsDirty = true;
				_allAspectsExtantDirty = true;
			}

            if (_tabletopAspectsDirty)
			{
				if (_tabletopAspects==null)
					_tabletopAspects=new AspectsDictionary();
				else
					_tabletopAspects.Clear();


				var tabletopStacks = _tabletop.GetElementStacksManager()?.GetStacks();
                if(tabletopStacks!=null)
                { 
                    foreach(var tabletopStack in tabletopStacks)
                    {
                        IAspectsDictionary stackAspects = tabletopStack.GetAspects();
                        IAspectsDictionary multipliedAspects = new AspectsDictionary();
                        //If we just count aspects, a stack of 10 cards only counts them once. I *think* this is the only place we need to worry about this rn,
                        //but bear it in mind in case there's ever a similar issue inside situations <--there is! if multiple cards are output, they stack.
                        //However! To complicate matters, if we're counting elements rather than aspects, there is already code in the stack to multiply aspect * quality, and we don't want to multiply it twice
                        foreach (var aspect in stackAspects)
                        {

                          if(aspect.Key==tabletopStack.EntityId)
                              multipliedAspects.Add(aspect.Key, aspect.Value);
                          else
                              multipliedAspects.Add(aspect.Key, aspect.Value * tabletopStack.Quantity);
                        }
                        _tabletopAspects.CombineAspects(multipliedAspects);
                    }


                    if (_enableAspectCaching)
                        _tabletopAspectsDirty = false;		// If left dirty the aspects will recalc every frame
                }
                _allAspectsExtantDirty = true;		// Force the aspects below to recalc
			}

			if (_allAspectsExtantDirty)
			{
				if (_allAspectsExtant == null)
					_allAspectsExtant=new AspectsDictionary();
				else
					_allAspectsExtant.Clear();

				var allSituations = Registry.Get<SituationsCatalogue>();
				foreach (var s in allSituations.GetRegisteredSituations())
                {
                    var stacksInSituation = new List<ElementStackToken>();
                    stacksInSituation.AddRange(s.GetStartingStacks());
                    stacksInSituation.AddRange(s.GetOngoingStacks());
                    stacksInSituation.AddRange(s.GetStoredStacks());
                    stacksInSituation.AddRange(s.GetOutputStacks());

                    foreach (var situationStack in stacksInSituation)
                    {
                        IAspectsDictionary stackAspects = situationStack.GetAspects();
                        IAspectsDictionary multipliedAspects = new AspectsDictionary();
                        //See notes above. We need to multiply aspects to take account of stack quantities here too.
                        foreach (var aspect in stackAspects)
                        {

                            if (aspect.Key == situationStack.EntityId)
                                multipliedAspects.Add(aspect.Key, aspect.Value);
                            else
                                multipliedAspects.Add(aspect.Key, aspect.Value * situationStack.Quantity);
                        }
                        _allAspectsExtant.CombineAspects(multipliedAspects);
                    }

                }
                _allAspectsExtant.CombineAspects(_tabletopAspects);

				if (_enableAspectCaching)
					_allAspectsExtantDirty = false;		// If left dirty the aspects will recalc every frame
			}

            AspectsInContext aspectsInContext=new AspectsInContext(aspectsInSituation, _tabletopAspects, _allAspectsExtant);

            return aspectsInContext;

        }

        public void GroupAllStacks()
        {
	        var stacks = _tabletop.GetElementStacksManager().GetStacks();
	        var groups = stacks.OfType<ElementStackToken>()
		        .GroupBy(e => e.EntityWithMutationsId, e => e)
		        .Select(group => group.OrderByDescending(e => e.Quantity).ToList());
	        var mergedStacks = false;
	        foreach (var group in groups)
	        {
		        var primaryStack = group.First();
		        var mergedStack = false;
		        foreach (var stack in group.Skip(1))
			        if (primaryStack.CanMergeWith(stack))
			        {
				        primaryStack.MergeIntoStack(stack);
				        mergedStack = true;
			        }

		        if (mergedStack)
			        StartCoroutine(primaryStack.PulseGlow());

		        mergedStacks |= mergedStack;
	        }
	        
	        if (mergedStacks)
		        SoundManager.PlaySfx("CardPutOnStack");
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

			#if ENABLE_ASPECT_CACHING
			if (GUI.Button( new Rect(Screen.width * 0.5f - 300f, 35f, 180f, 20f), "Aspect caching: " + (_enableAspectCaching?"ON":"OFF") ))
			{
				_enableAspectCaching = !_enableAspectCaching;		// Click
			}
			#endif
#endif

		}


        public void NotifyStacksChanged()
        {
          NotifyAspectsDirty();
        }

        public void WhenSettingUpdated(object newValue)
        {
            SetAutosaveInterval(newValue is float ? (float)newValue : 0);
        }

    }


}
