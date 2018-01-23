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


namespace Assets.CS.TabletopUI
{
    public class TabletopManager : MonoBehaviour,IStacksChangeSubscriber {

        [Header("Game Control")]
        [SerializeField] private Heart _heart;
        [SerializeField] private SpeedController _speedController;
        [SerializeField] private CardAnimationController _cardAnimationController;
        [SerializeField] private EndGameAnimController _endGameAnimController;

        [Header("Tabletop")]
        [SerializeField] public Tabletop _tabletop;
        [SerializeField] TabletopBackground _background;
        [SerializeField] private Limbo Limbo;

        [Header("Mansus Map")]
        [SerializeField] private MapController _mapController;
        [SerializeField] public MapContainsTokens mapContainsTokens;
        [SerializeField] TabletopBackground mapBackground;
        [SerializeField] MapAnimation mapAnimation;

        [Header("Drag & Window")]
        [SerializeField] private RectTransform draggableHolderRectTransform;
        [SerializeField] Transform tableLevelTransform;
        [SerializeField] Transform windowLevelTransform;

        [Header("Options Bar & Notes")]
        [SerializeField] private StatusBar StatusBar;

        [SerializeField] private DebugTools debugTools;
        [SerializeField] private BackgroundMusic backgroundMusic;
        [SerializeField] private HotkeyWatcher _hotkeyWatcher;

        [SerializeField] private Notifier _notifier;
        [SerializeField] private OptionsPanel _optionsPanel;
        [SerializeField] private ElementOverview _elementOverview;

        private SituationBuilder _situationBuilder;

        public void Update()
        {
            _hotkeyWatcher.WatchForGameplayHotkeys();
            _cardAnimationController.CheckForCardAnimations();
        }

        public void NotifyStacksChanged()
        {
            _elementOverview.UpdateDisplay();
        }

        void Start()
        {
            _situationBuilder = new SituationBuilder(tableLevelTransform, windowLevelTransform);            

            //register everything used gamewide
            SetupServices(_situationBuilder,_tabletop);
            //we hand off board functions to individual controllers
            InitialiseSubControllers(_speedController, _hotkeyWatcher, _cardAnimationController, _mapController, _endGameAnimController);
            InitialiseListeners();

            // Make sure dragging is reenabled
            DraggableToken.draggingEnabled = true;

            if (SceneManager.GetActiveScene().name == "Tabletop-w-Map") //hack while Martin's working in test scene
            {
                mapAnimation.Init();
                mapContainsTokens.gameObject.SetActive(false);
                mapBackground.onDropped += HandleOnMapBackgroundDropped;
            }

            BeginGame(_situationBuilder);

            _heart.StartBeatingWithDefaultValue();
        }

        /// <summary>
        /// if a game exists, load it; otherwise, create a fresh state and setup
        /// </summary>
        private void BeginGame(SituationBuilder builder)
        {

            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());

            if (saveGameManager.DoesGameSaveExist() && saveGameManager.IsSavedGameActive())
                LoadGame();
            else
            {
                SetupNewBoard(builder);
                var populatedCharacter = Registry.Retrieve<Character>(); //should just have been set above, but let's keep this clean
                Registry.Retrieve<ICompendium>().ReplaceTokens(populatedCharacter);
            }

      
        }

        private void InitialiseSubControllers(SpeedController speedController, HotkeyWatcher hotkeyWatcher, 
                                              CardAnimationController cardAnimationController, MapController mapController,
                                              EndGameAnimController endGameAnimController)
        {
            speedController.Initialise(_heart);
            hotkeyWatcher.Initialise(_speedController, debugTools, _optionsPanel);
            cardAnimationController.Initialise(_tabletop.GetElementStacksManager());
            mapController.Initialise(mapContainsTokens, mapBackground, mapAnimation);
            endGameAnimController.Initialise();
        }

        private void InitialiseListeners()
        {
            // Init Listeners to pre-existing DisplayHere Objects
            _background.onDropped += HandleOnTableDropped;
            _background.onClicked += HandleOnTableClicked;
            DraggableToken.onChangeDragState += HandleDragStateChanged;
        }

        private void SetupServices(SituationBuilder builder,Tabletop containsTokens)
        {
            var registry = new Registry();
            var compendium = new Compendium();
            var character = new Character();
            var choreographer=new Choreographer(containsTokens, builder,tableLevelTransform,windowLevelTransform);
            var situationsCatalogue=new SituationsCatalogue();
            var elementStacksCatalogue=new StackManagersCatalogue();
            //ensure we get updates about stack changes
            elementStacksCatalogue.Subscribe(this);
            

            var draggableHolder = new DraggableHolder(draggableHolderRectTransform);

            registry.Register<ICompendium>(compendium);
            registry.Register<IDraggableHolder>(draggableHolder);
            registry.Register<IDice>(new Dice(debugTools));
            registry.Register<TabletopManager>(this);
            registry.Register<SituationBuilder>(builder);
            registry.Register<INotifier>(_notifier);
            registry.Register<Character>(character);
            registry.Register<Choreographer>(choreographer);
            registry.Register<MapController>(_mapController);
            registry.Register<Limbo>(Limbo);
            registry.Register<SituationsCatalogue>(situationsCatalogue);
            registry.Register<StackManagersCatalogue>(elementStacksCatalogue);

            var contentImporter = new ContentImporter();
            contentImporter.PopulateCompendium(compendium);
            Limbo.Initialise();



        }

        private void OnDestroy() {
            // Sattic event so make sure to de-init once this object is destroyed
            DraggableToken.onChangeDragState -= HandleDragStateChanged;
        }

        public void SetPausedState(bool paused)
        {
            _speedController.SetPausedState(paused);
        }

        public void SetupNewBoard(SituationBuilder builder)
        {

            var chosenLegacy = CrossSceneState.GetChosenLegacy();
            if (chosenLegacy == null)
            {
                NoonUtility.Log("No initial Legacy specified");
                chosenLegacy = Registry.Retrieve<ICompendium>().GetAllLegacies().First();
            }

            builder.CreateInitialTokensOnTabletop();
            ProvisionStartingElements(chosenLegacy,Registry.Retrieve<Choreographer>());
            SetStartingCharacterInfo(chosenLegacy,CrossSceneState.GetDefunctCharacterName());
            StatusBar.UpdateCharacterDetailsView(Registry.Retrieve<Character>());

            DealStartingDecks();

            _notifier.ShowNotificationWindow(chosenLegacy.Label, chosenLegacy.StartDescription, 30);
        }

        private void SetStartingCharacterInfo(Legacy chosenLegacy,string previousCharacterName)
        {
            Character newCharacter = Registry.Retrieve<Character>();
            newCharacter.Name = "[click to name]";
            newCharacter.Profession = chosenLegacy.Label;
            newCharacter.PreviousCharacterName = previousCharacterName;
        }

        private void DealStartingDecks()
        {
            IGameEntityStorage character = Registry.Retrieve<Character>();
            var compendium = Registry.Retrieve<ICompendium>();
            foreach(var ds in compendium.GetAllDeckSpecs())
            { 
                IDeckInstance di=new DeckInstance(ds);
                character.DeckInstances.Add(di);
                di.Reset();
            }

        }


        public void ProvisionStartingElements(Legacy chosenLegacy,Choreographer choreographer)
        {
            AspectsDictionary startingElements = new AspectsDictionary();
            
                startingElements.CombineAspects(chosenLegacy.Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.
           
            foreach (var e in startingElements)
            {
                ElementStackToken token = _tabletop.GetTokenTransformWrapper().ProvisionElementStackAsToken(e.Key, e.Value,Source.Existing());
               choreographer.ArrangeTokenOnTable(token);
            }
        }

        public void BeginNewSituation(SituationCreationCommand scc)
        {
            Registry.Retrieve<Choreographer>().BeginNewSituation(scc);

        }

        public void ClearGameState(Heart h, IGameEntityStorage s,Tabletop tc)
        {
            h.Clear();
            s.DeckInstances=new List<IDeckInstance>();
       
            
            foreach(var sc in Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations())
                sc.Retire();

            foreach (var element in tc.GetElementStacksManager().GetStacks())
                element.Retire(true); //looks daft but pretty on reset
        }



        public void RestartGame()
        {
            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());
             saveGameManager.SaveInactiveGame();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }


        public void EndGame(Ending ending, SituationController endingSituation)
        {
            var ls = new LegacySelector(Registry.Retrieve<ICompendium>());
            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());

            CrossSceneState.SetCurrentEnding(ending);
            CrossSceneState.SetDefunctCharacter(Registry.Retrieve<Character>());
            CrossSceneState.SetAvailableLegacies(ls.DetermineLegacies(ending, null));

            saveGameManager.SaveInactiveGame();

            string animName;

            if (string.IsNullOrEmpty(ending.Anim))
                animName = "DramaticLight";
            else
                animName = ending.Anim;

            // TODO: Get effect name from ending?
            _endGameAnimController.TriggerEnd((SituationToken) endingSituation.situationToken, animName);
        }

    	public HashSet<TokenAndSlot> FillTheseSlotsWithFreeStacks(HashSet<TokenAndSlot> slotsToFill)
        {
            var unprocessedSlots = new HashSet<TokenAndSlot>();
            foreach (var tokenSlotPair in slotsToFill)
            {
                if (!tokenSlotPair.RecipeSlot.Equals(null)) //it hasn't been destroyed
                {
                    if (tokenSlotPair.RecipeSlot.GetElementStackInSlot() == null)
                    {
                        var stack = findStackForSlotSpecification(tokenSlotPair.RecipeSlot.GoverningSlotSpecification);
                        if (stack != null)
                        {
                            stack.SplitAllButNCardsToNewStack(1);
						Registry.Retrieve<Choreographer>().MoveElementToSituationSlot(stack as ElementStackToken, tokenSlotPair); // NOTE: Needs token
                        }
                        else
                            unprocessedSlots.Add(tokenSlotPair);
                    }
                }
            }
            return unprocessedSlots;
        }



        private IElementStack findStackForSlotSpecification(SlotSpecification slotSpec)
        {
            var stacks = _tabletop.GetElementStacksManager().GetStacks();
            foreach (var stack in stacks)
                if (slotSpec.GetSlotMatchForAspects(stack.GetAspects()).MatchType == SlotMatchForAspectsType.Okay)
                    return stack;

            return null;
        }



        void HandleOnTableDropped()
        {
            if (DraggableToken.itemBeingDragged != null)
            {
                DraggableToken.SetReturn(false,"dropped on the background");

                if (DraggableToken.itemBeingDragged is SituationToken)
                    _tabletop.DisplaySituationTokenOnTable((SituationToken) DraggableToken.itemBeingDragged);
                else if (DraggableToken.itemBeingDragged is ElementStackToken)
                    _tabletop.GetElementStacksManager().AcceptStack(((ElementStackToken) DraggableToken.itemBeingDragged));
                else
                    throw new NotImplementedException("Tried to put something weird on the table");


                SoundManager.PlaySfx("CardDrop");
            }
        }

        void HandleOnTableClicked()
        {
            //Close all open windows if we're not dragging (multi tap stuff)
            if (DraggableToken.itemBeingDragged == null)
                CloseAllSituationWindowsExcept(null);

        }

        public void CloseAllSituationWindowsExcept(string exceptTokenId)
        {
            var situationControllers = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();

            foreach (var controller in situationControllers)
            {
                if (controller.GetActionId()!=exceptTokenId)
                    controller.CloseSituation();
            }
        }


        void HandleOnMapBackgroundDropped() {
            if (DraggableToken.itemBeingDragged != null) {

                DraggableToken.SetReturn(false, "dropped on the map background");
                DraggableToken.itemBeingDragged.DisplayAtTableLevel();
                mapContainsTokens.GetTokenTransformWrapper().DisplayHere(DraggableToken.itemBeingDragged);

                SoundManager.PlaySfx("CardDrop");
            }
        }

        public void LoadGame()
        {
            ICompendium compendium = Registry.Retrieve<ICompendium>();
            IGameEntityStorage storage = Registry.Retrieve<Character>();

           _speedController.SetPausedState(true);
            var saveGameManager = new GameSaveManager(new GameDataImporter(compendium), new GameDataExporter());
            //try
            //{
                var htSave = saveGameManager.RetrieveHashedSaveFromFile();
                ClearGameState(_heart, storage, _tabletop);
                saveGameManager.ImportHashedSaveToState(_tabletop, storage, htSave);
                StatusBar.UpdateCharacterDetailsView(storage);
                _notifier.ShowNotificationWindow("Where were we?", " - we have loaded the game.");

            //}
            //catch (Exception e)
            //{
            //    _notifier.ShowNotificationWindow("Couldn't load game - ", e.Message);
            //}
           _speedController.SetPausedState(false);
        }

        public void SaveGame(bool withNotification)
        {
            _heart.StopBeating();

            //Close all windows and dump tokens to desktop before saving.
            //We don't want or need to track half-started situations.
            var allSituationControllers = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();
            foreach (var s in allSituationControllers)
                s.CloseSituation();

           // try
          //  {
                var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());
                saveGameManager.SaveActiveGame(_tabletop, Registry.Retrieve<Character>());
                if (withNotification)
                    _notifier.ShowNotificationWindow("SAVED THE GAME", "BUT NOT THE WORLD");

            //}
            //catch (Exception e)
            //{

          //      _notifier.ShowNotificationWindow("Couldn't save game - ", e.Message); ;
           // }

            _heart.ResumeBeating();
        }

        public void ShowDestinationsForStack(IElementStack stack)
        {
            var situations = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();

            for (int i = 0; i < situations.Count; i++) {
                if (situations[i].IsOpen)
                    situations[i].ShowDestinationsForStack(stack);

                // null means no stack, so highlight if we have a stack and it can be dropped here

                situations[i].situationToken.SetGlowColor(UIStyle.TokenGlowColor.HighlightPink);
                situations[i].situationToken.ShowGlow(stack != null && situations[i].CanTakeDroppedToken(stack)); 
            }

            if (mapContainsTokens != null)
                mapContainsTokens.ShowDestinationsForStack(stack);
        }

        public void DecayStacksOnTable(float interval)
        {
            var decayingStacks = _tabletop.GetElementStacksManager().GetStacks().Where(s => s.Decays);
            foreach(var d in decayingStacks)
                d.Decay(interval);
        }

        private void HandleDragStateChanged(bool isDragging) {
            var draggedElement = DraggableToken.itemBeingDragged as ElementStackToken;
            
            // not dragging a stack? then do nothing. _tabletop was destroyed (end of game?)
            if (draggedElement == null || _tabletop == null)
                return;

            var tabletopStacks = _tabletop.GetElementStacksManager().GetStacks();
            ElementStackToken token;

            foreach (var stack in tabletopStacks) {
                if (stack.Id != draggedElement.Id || stack.Defunct)
                    continue;

                if (!isDragging || stack.AllowMerge()) {
                    token = stack as ElementStackToken;

                    if (token != null && token != draggedElement) {
                        token.SetGlowColor(UIStyle.TokenGlowColor.HighlightPink);
                        token.ShowGlow(isDragging, false);
                    }
                }
            }
        }

        public void SignalImpendingDoom(ISituationAnchor situationToken)
        {
            //including the situationToken so we can zoom to it or otherwise signal it at some point, but for now, let's just play some scary music
            backgroundMusic.PlayImpendingDoom();
        }

   
    }

}
