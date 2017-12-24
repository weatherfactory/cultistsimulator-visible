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
    public class TabletopManager : MonoBehaviour {

        [Header("Game Control")]
        [SerializeField] private Heart _heart;
        [SerializeField] private SpeedController _speedController;
        [SerializeField] private CardAnimationController _cardAnimationController;

        [Header("Tabletop")]
        [SerializeField] public TabletopContainer _tabletopContainer;
        [SerializeField] TabletopBackground _background;
        [SerializeField] private Limbo Limbo;


        [Header("Mansus Map")]
        [SerializeField] private MapController _mapController;
        [SerializeField] public MapContainer mapContainer;
        [SerializeField] TabletopBackground mapBackground;
        [SerializeField] MapAnimation mapAnimation;

        [Header("Drag & Window")]
        [SerializeField] private RectTransform draggableHolderRectTransform;
        [SerializeField] Transform windowLevel;

        [Header("Options Bar & Notes")] [SerializeField] private StatusBar StatusBar;

        [SerializeField] private DebugTools debugTools;
        [SerializeField] private BackgroundMusic backgroundMusic;
        [SerializeField] private HotkeyWatcher _hotkeyWatcher;

        [SerializeField] private Notifier _notifier;
        [SerializeField] private OptionsPanel _optionsPanel;
        [SerializeField] private ElementOverview _elementOverview;

        private TabletopObjectBuilder _tabletopObjectBuilder;



        public void Update()
        {
            _hotkeyWatcher.WatchForHotkeys();
            _elementOverview.UpdateDisplay(_tabletopContainer.GetElementStacksManager(),
                Registry.Retrieve<TokensCatalogue>().GetRegisteredSituations());
            _cardAnimationController.CheckForCardAnimations();
        }


        void Start()
        {
            _tabletopObjectBuilder = new TabletopObjectBuilder(_tabletopContainer.transform, windowLevel);
            
            //register everything used gamewide
            SetupServices(_tabletopObjectBuilder,_tabletopContainer);
            //we hand off board functions to individual controllers
            InitialiseSubControllers(_speedController, _hotkeyWatcher, _cardAnimationController,_mapController);
            InitialiseListeners();

            if (SceneManager.GetActiveScene().name == "Tabletop-w-Map") //hack while Martin's working in test scene
            {
                
                mapAnimation.Init();
                mapContainer.gameObject.SetActive(false);
                mapBackground.onDropped += HandleOnMapBackgroundDropped;
            }

            BeginGame(_tabletopObjectBuilder);

            _heart.StartBeatingWithDefaultValue();

        }

        /// <summary>
        /// if a game exists, load it; otherwise, create a fresh state and setup
        /// </summary>
        private void BeginGame(TabletopObjectBuilder builder)
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

        private void InitialiseSubControllers(SpeedController speedController, HotkeyWatcher hotkeyWatcher, CardAnimationController cardAnimationController,MapController mapController)
        {
            speedController.Initialise(_heart);
            hotkeyWatcher.Initialise(_speedController, debugTools, _optionsPanel);
            cardAnimationController.Initialise(_tabletopContainer.GetElementStacksManager());
            mapController.Initialise(mapContainer, mapBackground, mapAnimation);
        }

        private void InitialiseListeners()
        {
            // Init Listeners to pre-existing Display Objects
            _background.onDropped += HandleOnTableDropped;
            _background.onClicked += HandleOnTableClicked;
            DraggableToken.onChangeDragState += HandleDragStateChanged;
        }

        private void SetupServices(TabletopObjectBuilder builder,TabletopContainer container)
        {
            var registry = new Registry();
            var compendium = new Compendium();
            var character = new Character();
            var choreographer=new Choreographer(container, builder);
            var situationsManager=new TokensCatalogue();

            var draggableHolder = new DraggableHolder(draggableHolderRectTransform);

            registry.Register<ICompendium>(compendium);
            registry.Register<IDraggableHolder>(draggableHolder);
            registry.Register<IDice>(new Dice());
            registry.Register<TabletopManager>(this);
            registry.Register<TabletopObjectBuilder>(builder);
            registry.Register<INotifier>(_notifier);
            registry.Register<Character>(character);
            registry.Register<Choreographer>(choreographer);
            registry.Register<MapController>(_mapController);
            registry.Register<Limbo>(Limbo);
            registry.Register<TokensCatalogue>(situationsManager);

            var contentImporter = new ContentImporter();
            contentImporter.PopulateCompendium(compendium);

            
        }

        private void OnDestroy() {
            // Sattic event so make sure to de-init once this object is destroyed
            DraggableToken.onChangeDragState -= HandleDragStateChanged;
        }

        public void SetPausedState(bool paused)
        {
            _speedController.SetPausedState(paused);
        }

        public void SetupNewBoard(TabletopObjectBuilder builder)
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
                ElementStackToken token = _tabletopContainer.GetTokenTransformWrapper().ProvisionElementStackAsToken(e.Key, e.Value,Source.Existing());
               choreographer.ArrangeTokenOnTable(token);
            }
        }

        public void BeginNewSituation(SituationCreationCommand scc)
        {
            Registry.Retrieve<Choreographer>().BeginNewSituation(scc);

        }

        public void ClearGameState(Heart h, IGameEntityStorage s,TabletopContainer tc)
        {
            h.Clear();
            s.DeckInstances=new List<IDeckInstance>();
       
            
            foreach(var sc in Registry.Retrieve<TokensCatalogue>().GetRegisteredSituations())
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



        public void EndGame(Ending ending)
        {
            CrossSceneState.SetCurrentEnding(ending);
            CrossSceneState.SetDefunctCharacter(Registry.Retrieve<Character>());
            var ls=new LegacySelector(Registry.Retrieve<ICompendium>());
            CrossSceneState.SetAvailableLegacies(ls.DetermineLegacies(ending, null));
            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());

            saveGameManager.SaveInactiveGame();

            SceneManager.LoadScene(SceneNumber.EndScene);

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
            var stacks = _tabletopContainer.GetElementStacksManager().GetStacks();
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

                if(DraggableToken.itemBeingDragged is SituationToken)
                    _tabletopContainer.PutOnTable((SituationToken) DraggableToken.itemBeingDragged);
                else if (DraggableToken.itemBeingDragged is ElementStackToken)
                    _tabletopContainer.PutOnTable((ElementStackToken) DraggableToken.itemBeingDragged);
                else
                    throw new NotImplementedException("Tried to put something weird on the table");


                SoundManager.PlaySfx("CardDrop");
            }
        }

        void HandleOnTableClicked()
        {
            //Close all open windows if we're not dragging (multi tap stuff)
            if (DraggableToken.itemBeingDragged == null)
                _tabletopContainer.CloseAllSituationWindowsExcept(null);

        }


        void HandleOnMapBackgroundDropped() {
            if (DraggableToken.itemBeingDragged != null) {

                DraggableToken.SetReturn(false, "dropped on the map background");
                DraggableToken.itemBeingDragged.DisplayOnTable();
                mapContainer.GetTokenTransformWrapper().Accept(DraggableToken.itemBeingDragged);

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
                ClearGameState(_heart, storage, _tabletopContainer);
                saveGameManager.ImportHashedSaveToState(_tabletopContainer, storage, htSave);
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
            var allSituationControllers = Registry.Retrieve<TokensCatalogue>().GetRegisteredSituations();
            foreach (var s in allSituationControllers)
                s.CloseSituation();

           // try
          //  {
                var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());
                saveGameManager.SaveActiveGame(_tabletopContainer, Registry.Retrieve<Character>());
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
            var openToken = _tabletopContainer.GetOpenToken();

            if (openToken !=null)
               openToken.ShowDestinationsForStack(stack);

            if (mapContainer != null)
                mapContainer.ShowDestinationsForStack(stack);
        }

        public void DecayStacksOnTable(float interval)
        {
            var decayingStacks = _tabletopContainer.GetElementStacksManager().GetStacks().Where(s => s.Decays);
            foreach(var d in decayingStacks)
                d.Decay(interval);
        }

        private void HandleDragStateChanged(bool isDragging) {

            var draggedElement = DraggableToken.itemBeingDragged as ElementStackToken;
            
            // not dragging a stack? then do nothing. _tabletopContainer was destroyed (end of game?)
            if (draggedElement == null || _tabletopContainer == null)
                return;

            var tabletopStacks = _tabletopContainer.GetElementStacksManager().GetStacks();
            ElementStackToken token;

            foreach (var stack in tabletopStacks) {
                if (stack.Id != draggedElement.Id || stack.Defunct)
                    continue;

                if (!isDragging || stack.AllowMerge()) {
                    token = stack as ElementStackToken;

                    if (token != null) {
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
