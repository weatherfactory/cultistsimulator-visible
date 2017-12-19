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


namespace Assets.CS.TabletopUI
{
    public class TabletopManager : MonoBehaviour {

        [Header("Game Control")]
        [SerializeField] private Heart _heart;
        [SerializeField] private SpeedController _speedController;
        [SerializeField] private CardAnimationController _cardAnimationController;

        [Header("Tabletop")]
        [SerializeField] public TabletopContainer TabletopContainer;
        [SerializeField] TabletopBackground _background;


        private TabletopObjectBuilder tabletopObjectBuilder;
        

        [Header("Mansus Map")]
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

        

        public void Update()
        {
            _hotkeyWatcher.WatchForHotkeys();

            _elementOverview.UpdateDisplay(TabletopContainer.GetElementStacksManager(),TabletopContainer.GetAllSituationTokens());

            _cardAnimationController.CheckForCardAnimations();

 
        }


        void Start()
        {
            var registry = new Registry();
            var compendium = new Compendium();
            var character=new Character();
            tabletopObjectBuilder = new TabletopObjectBuilder(TabletopContainer.transform, windowLevel);
            var draggableHolder = new DraggableHolder(draggableHolderRectTransform);

            registry.Register<ICompendium>(compendium);
            registry.Register<IDraggableHolder>(draggableHolder);
            registry.Register<IDice>(new Dice());
            registry.Register<TabletopManager>(this);
            registry.Register<TabletopObjectBuilder>(tabletopObjectBuilder);
            registry.Register<INotifier>(_notifier);
            registry.Register<Character>(character);


            var contentImporter = new ContentImporter();
            contentImporter.PopulateCompendium(compendium);



            _speedController.Initialise(_heart);
            _hotkeyWatcher.Initialise(_speedController, debugTools,_optionsPanel);
            _cardAnimationController.Initialise(TabletopContainer.GetElementStacksManager());

            

            if (SceneManager.GetActiveScene().name == "Tabletop-w-Map") //hack while Martin's working in test scene
            {
                mapAnimation.Init();
                mapContainer.gameObject.SetActive(false);
                mapBackground.onDropped += HandleOnMapBackgroundDropped;
            }
            // Init Listeners to pre-existing Display Objects
            _background.onDropped += HandleOnBackgroundDropped;
            _background.onClicked += HandleOnBackgroundClicked;

            DraggableToken.onChangeDragState += HandleDragStateChanged;

            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());

            if (saveGameManager.DoesGameSaveExist() && saveGameManager.IsSavedGameActive())
                LoadGame();
            else
            {
                SetupNewBoard();
                var populatedCharacter =Registry.Retrieve<Character>(); //should just have been set above, but let's keep this clean
                compendium.ReplaceTokens(populatedCharacter);
            }

            _heart.StartBeatingWithDefaultValue();


        }

        private void OnDestroy() {
            // Sattic event so make sure to de-init once this object is destroyed
            DraggableToken.onChangeDragState -= HandleDragStateChanged;
        }

        public void SetPausedState(bool paused)
        {
            _speedController.SetPausedState(paused);
        }

        public void SetupNewBoard()
        {

            var chosenLegacy = CrossSceneState.GetChosenLegacy();
            if (chosenLegacy == null)
            {
                NoonUtility.Log("No initial Legacy specified");
                chosenLegacy = Registry.Retrieve<ICompendium>().GetAllLegacies().First();
            }

            tabletopObjectBuilder.CreateInitialTokensOnTabletop();
            ProvisionStartingElements(chosenLegacy);
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


        private void ProvisionStartingElements(Legacy chosenLegacy)
        {
            AspectsDictionary startingElements = new AspectsDictionary();
            
                startingElements.CombineAspects(chosenLegacy.Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.
           

            foreach (var e in startingElements)
            {
                ElementStackToken token = TabletopContainer.GetTokenTransformWrapper().ProvisionElementStackAsToken(e.Key, e.Value,Source.Existing());
                ArrangeTokenOnTable(token);
            }
        }

        public void BeginNewSituation(SituationCreationCommand scc)
        {

            if (scc.Recipe == null)
                throw new ApplicationException("DON'T PASS AROUND SITUATIONCREATIONCOMMANDS WITH RECIPE NULL");
            //if new situation is beginning with an existing verb: do not action the creation.
            //This may break some functionality initially because of the heavy use of 'x' as the default verb
            //but is probably necessary to avoid multiple menace tokens and move away from dependency on maxoccurrences
            //oh: I could have an scc property which is a MUST CREATE override


            var existingToken = TabletopContainer.GetAllSituationTokens().SingleOrDefault(t => t.Id == scc.Recipe.ActionId);
            //grabbing existingtoken: just in case some day I want to, e.g., add additional tokens to an ongoing one rather than silently fail the attempt.
            if(existingToken!=null)
            { 
                NoonUtility.Log("Tried to create " + scc.Recipe.Id + " for verb " + scc.Recipe.ActionId + " but that verb is already active.");
                //end execution here
                return;
            }
            var token = tabletopObjectBuilder.CreateTokenWithAttachedControllerAndSituation(scc);

            //if token has been spawned from an existing token, animate its appearance
			if (scc.SourceToken != null) {
				var tokenAnim = token.gameObject.AddComponent<TokenAnimation>();
				tokenAnim.onAnimDone += SituationAnimDone;
				tokenAnim.SetPositions(scc.SourceToken.RectTransform.anchoredPosition3D, GetFreeTokenPosition(token, new Vector2(0, -250f)));
				tokenAnim.SetScaling(0f, 1f);
				tokenAnim.StartAnim();
			}
			else {
				ArrangeTokenOnTable(token);
			}
        }

		void SituationAnimDone(DraggableToken token) {
			TabletopContainer.PutOnTable(token);
		}


        public void ShowMansusMap(Transform effectCenter, bool show = true) {
            if (mapAnimation.CanShow(show) == false)
                return;

            // TODO: should probably lock interface? No zoom, no tabletop interaction

            mapAnimation.onAnimDone += OnMansusMapAnimDone;
            mapAnimation.SetCenterForEffect(effectCenter);
            mapAnimation.Show(show); // starts coroutine that calls onManusMapAnimDone when done
            mapContainer.Show(show);
        }

        void OnMansusMapAnimDone(bool show) {
            mapAnimation.onAnimDone -= OnMansusMapAnimDone;
            // TODO: should probably unlock interface? No zoom, no tabletop interaction
        }

        public void HideMansusMap(Transform effectCenter, IElementStack stack) {
            Debug.Log("Dropped Stack " + (stack != null ? stack.Id : "NULL"));
            ShowMansusMap(effectCenter, false);
        }


        public void ClearGameState(Heart h, IGameEntityStorage s,TabletopContainer tc)
        {
            h.Clear();
            s.DeckInstances=new List<IDeckInstance>();
       
            foreach (var situation in tc.GetAllSituationTokens())
                situation.Retire();

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
							MoveElementToSituationSlot(stack as ElementStackToken, tokenSlotPair); // NOTE: Needs token
                        }
                        else
                            unprocessedSlots.Add(tokenSlotPair);
                    }
                }
            }
            return unprocessedSlots;
        }

		void MoveElementToSituationSlot(ElementStackToken stack, TokenAndSlot tokenSlotPair) {
			var stackAnim = stack.gameObject.AddComponent<TokenAnimationToSlot>();
			stackAnim.onElementSlotAnimDone += ElementGreedyAnimDone;
			stackAnim.SetPositions(stack.RectTransform.anchoredPosition3D, tokenSlotPair.Token.GetOngoingSlotPosition());
			stackAnim.SetScaling(1f, 0.35f);
			stackAnim.SetTargetSlot(tokenSlotPair);
			stackAnim.StartAnim(0.2f);
		}

		void ElementGreedyAnimDone(ElementStackToken element, TokenAndSlot tokenSlotPair) {
            if(!tokenSlotPair.RecipeSlot.Equals(null))
			tokenSlotPair.RecipeSlot.AcceptStack(element);
		}

        private IElementStack findStackForSlotSpecification(SlotSpecification slotSpec)
        {
            var stacks = TabletopContainer.GetElementStacksManager().GetStacks();
            foreach (var stack in stacks)
                if (slotSpec.GetSlotMatchForAspects(stack.GetAspects()).MatchType == SlotMatchForAspectsType.Okay)
                    return stack;

            return null;
        }

        public IEnumerable<ISituationAnchor> GetAllSituationTokens()
        {
            return TabletopContainer.GetAllSituationTokens();
        }

        public void ArrangeTokenOnTable(DraggableToken token)
        {
			token.transform.localPosition = GetFreeTokenPosition(token, new Vector2(0, -250f));
            TabletopContainer.PutOnTable(token);
        }

        //we place stacks horizontally rather than vertically
        public void ArrangeTokenOnTable(ElementStackToken stack)
        {
			stack.transform.localPosition = GetFreeTokenPosition(stack, new Vector2(-100f, 0f));
            TabletopContainer.PutOnTable(stack);
        }

		private Vector3 GetFreeTokenPosition(DraggableToken token, Vector2 candidateOffset) {
			Vector2 marginPixels = new Vector2(50f, 50f);
		    Vector2 candidatePos = new Vector2(0f, 250f);

			float arbitraryYCutoffPoint = -1000;

			while (TokenOverlapsPosition(token, marginPixels, candidatePos) && candidatePos.y > arbitraryYCutoffPoint) 
				candidatePos += candidateOffset;

			return candidatePos;
		}

		private bool TokenOverlapsPosition(DraggableToken token, Vector2 marginPixels, Vector2 candidatePos)
        {
            foreach (var t in TabletopContainer.GetTokenTransformWrapper().GetTokens())
            {
                if (token != t
					&& candidatePos.x - t.transform.localPosition.x < marginPixels.x
					&& candidatePos.x - t.transform.localPosition.x > -marginPixels.x
					&& candidatePos.y - t.transform.localPosition.y < marginPixels.y
					&& candidatePos.y - t.transform.localPosition.y > -marginPixels.y)
                { 
                     return true;
                }
            
            }

            return false;
        }




        void HandleOnBackgroundDropped()
        {
            // NOTE: This puts items back on the background. We need this in more cases. Should be a method
            if (DraggableToken.itemBeingDragged != null)
            {
                // Maybe check for item type here via GetComponent<Something>() != null?
                DraggableToken.SetReturn(false,"dropped on the background");
                // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
                // This currently treats everything as a token, even dragged windows. Instead draggables should have a type that can be checked for when returning token to default layer?
                // Dragged windows should not change in height during/after dragging, since they float by default

                //tabletopContainer.PutOnTable(DraggableToken.itemBeingDragged); // Make sure to parent back to the tabletop
                DraggableToken.itemBeingDragged.DisplayOnTable();
                TabletopContainer.GetTokenTransformWrapper().Accept(DraggableToken.itemBeingDragged);

                SoundManager.PlaySfx("CardDrop");
            }
        }

        void HandleOnBackgroundClicked()
        {
            //Close all open windows if we're not dragging (multi tap stuff)
            if (DraggableToken.itemBeingDragged == null)
                TabletopContainer.CloseAllSituationWindowsExcept(null);

        }


        void HandleOnMapBackgroundDropped() {
            // NOTE: This puts items back on the background. We need this in more cases. Should be a method
            if (DraggableToken.itemBeingDragged != null) {
                // Maybe check for item type here via GetComponent<Something>() != null?
                DraggableToken.SetReturn(false, "dropped on the map background");
                // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
                // This currently treats everything as a token, even dragged windows. Instead draggables should have a type that can be checked for when returning token to default layer?
                // Dragged windows should not change in height during/after dragging, since they float by default

                //tabletopContainer.PutOnTable(DraggableToken.itemBeingDragged); // Make sure to parent back to the tabletop
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
            try
            {
                var htSave = saveGameManager.RetrieveHashedSaveFromFile();
                ClearGameState(_heart, storage, TabletopContainer);
                saveGameManager.ImportHashedSaveToState(TabletopContainer, storage, htSave);
                StatusBar.UpdateCharacterDetailsView(storage);
                _notifier.ShowNotificationWindow("Where were we?", " - we have loaded the game.");

            }
            catch (Exception e)
            {
                _notifier.ShowNotificationWindow("Couldn't load game - ", e.Message);
            }
           _speedController.SetPausedState(false);
        }

        public void SaveGame(bool withNotification)
        {
            _heart.StopBeating();

            //Close all windows and dump tokens to desktop before saving.
            //We don't want or need to track half-started situations.
            var allSituationTokens = TabletopContainer.GetAllSituationTokens();
            foreach (var t in allSituationTokens)
                t.SituationController.CloseSituation();

            try
            {
                var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());
                saveGameManager.SaveActiveGame(TabletopContainer, Registry.Retrieve<Character>());
                if (withNotification)
                    _notifier.ShowNotificationWindow("SAVED THE GAME", "BUT NOT THE WORLD");

            }
            catch (Exception e)
            {

                _notifier.ShowNotificationWindow("Couldn't save game - ", e.Message); ;
            }

            _heart.ResumeBeating();
        }

        public void ShowDestinationsForStack(IElementStack stack)
        {
            var openToken = TabletopContainer.GetOpenToken();

            if (openToken !=null)
               openToken.ShowDestinationsForStack(stack);

            if (mapContainer != null)
                mapContainer.ShowDestinationsForStack(stack);
        }

        public void DecayStacksOnTable(float interval)
        {
            var decayingStacks = TabletopContainer.GetElementStacksManager().GetStacks().Where(s => s.Decays);
            foreach(var d in decayingStacks)
                d.Decay(interval);
        }

        private void HandleDragStateChanged(bool isDragging) {
            var draggedElement = DraggableToken.itemBeingDragged as ElementStackToken;
            
            // not dragging a stack? then do nothing. TabletopContainer was destroyed (end of game?)
            if (draggedElement == null || TabletopContainer == null)
                return;

            var tabletopStacks = TabletopContainer.GetElementStacksManager().GetStacks();
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
