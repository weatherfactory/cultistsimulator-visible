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
        [SerializeField]
        private Heart heart;

        [Header("Tabletop")]
        [SerializeField] public TabletopContainer tabletopContainer;
        [SerializeField] TabletopBackground background;
        [SerializeField] float timeBetweenAnims = 5f;
        [SerializeField] float timeBetweenAnimsVariation = 1f;

        private TabletopObjectBuilder tabletopObjectBuilder;
        private float nextAnimTime;
        private string lastAnimID; // to not animate the same twice. Keep palyer on their toes

        [Header("Mansus Map")]
        [SerializeField] public MapContainer mapContainer;
        [SerializeField] TabletopBackground mapBackground;
        [SerializeField] MapAnimation mapAnimation;

        [Header("Drag & Window")]
        [SerializeField] private RectTransform draggableHolderRectTransform;
        [SerializeField] Transform windowLevel;

        [Header("Options Bar & Notes")] [SerializeField] private StatusBar StatusBar;
        [SerializeField] private PauseButton pauseButton;

        [SerializeField] private Button normalSpeedButton;
        [SerializeField] private Button fastForwardButton;
        [SerializeField] private DebugTools debugTools;
        [SerializeField] private BackgroundMusic backgroundMusic;

        [SerializeField] private Notifier notifier;
        [SerializeField] private OptionsPanel optionsPanel;
        [SerializeField] private ElementOverview elementOverview;

        private readonly Color activeSpeedColor = new Color32(147, 225, 239, 255);
        private readonly Color inactiveSpeedColor = Color.white;





        // A total number of 4 supported by the bar currently. Not more, not fewer
        private string[] overviewElementIds = new string[] {
            "health", "passion", "reason", "funds"
        };

        public void Update()
        {
            if (Input.GetKeyDown("`") || (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab)))
                debugTools.gameObject.SetActive(!debugTools.isActiveAndEnabled);

            if (!debugTools.isActiveAndEnabled)
            {
               //...it's nice to be able to type N and M

                if (Input.GetKeyDown(KeyCode.N))
                    SetNormalSpeed();

                if (Input.GetKeyDown(KeyCode.M))
                    SetFastForward();

            }

            if (Input.GetKeyDown(KeyCode.Space))
                TogglePause();

            if (Input.GetKeyDown(KeyCode.Escape))
                optionsPanel.ToggleVisibility();

            UpdateElementOverview();

            if (Time.time >= nextAnimTime) { 
                TriggerArtAnimation();
                SetNextAnimTime();
            }
        }

        public void UpdateCompendium(ICompendium compendium)
        {
            var contentImporter = new ContentImporter();
            contentImporter.PopulateCompendium(compendium);
            foreach (var p in contentImporter.GetContentImportProblems())
                Debug.Log(p.Description);
        }
        
        void Start()
        {
            var registry = new Registry();
            var compendium = new Compendium();
            var character=new Character();
            registry.Register<ICompendium>(compendium);
            UpdateCompendium(compendium);
            SetNextAnimTime(); // sets the first animation for the tabletop Controller

            tabletopObjectBuilder = new TabletopObjectBuilder(tabletopContainer.transform, windowLevel);
         
            registry.Register<IDraggableHolder>(new DraggableHolder(draggableHolderRectTransform));
            registry.Register<IDice>(new Dice());
            registry.Register<TabletopManager>(this);
            registry.Register<TabletopObjectBuilder>(tabletopObjectBuilder);
            registry.Register<INotifier>(notifier);
            registry.Register<Character>(character);

            // setup map
            mapAnimation.Init();
            mapContainer.gameObject.SetActive(false);
            mapBackground.onDropped += HandleOnMapBackgroundDropped;

            // Init Listeners to pre-existing Display Objects
            background.onDropped += HandleOnBackgroundDropped;
            background.onClicked += HandleOnBackgroundClicked;

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

            heart.StartBeating(0.05f);
            normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;

            //replace all tokens in elements and recipes with appropriate starting state values.
        }

        private void OnDestroy() {
            // Sattic event so make sure to de-init once this object is destroyed
            DraggableToken.onChangeDragState -= HandleDragStateChanged;
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

            notifier.ShowNotificationWindow(chosenLegacy.Label, chosenLegacy.StartDescription, 30);
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
                ElementStackToken token = tabletopContainer.GetTokenTransformWrapper().ProvisionElementStackAsToken(e.Key, e.Value,Source.Existing());
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


            var existingToken = tabletopContainer.GetAllSituationTokens().SingleOrDefault(t => t.Id == scc.Recipe.ActionId);
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
			tabletopContainer.PutOnTable(token);
		}


        public void ShowMansusMap(bool show = true) {
            if (mapAnimation.CanShow(show) == false)
                return;

            // TODO: should probably lock interface? No zoom, no tabletop interaction

            mapAnimation.onAnimDone += OnMansusMapAnimDone;
            mapAnimation.Show(show); // starts coroutine that calls onManusMapAnimDone when done
        }

        void OnMansusMapAnimDone(bool show) {
            // TODO: should probably unlock interface? No zoom, no tabletop interaction

            mapAnimation.onAnimDone -= OnMansusMapAnimDone;

            if (show)
                mapContainer.Show(show);
        }

        public void HideMansusMap(IElementStack stack) {
            Debug.Log("Dropped Stack " + (stack != null ? stack.Id : "NULL"));

            // TODO: should probably lock interface? No zoom, no tabletop interaction

            mapAnimation.onAnimDone += OnMansusMapAnimDone;
            mapAnimation.Show(false); // starts coroutine that calls onManusMapAnimDone when done
            mapContainer.Show(false);
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

        public void SetPausedState(bool pause)
        {
            if (pause)
            {
                heart.StopBeating();
                pauseButton.SetPausedText(true);
                pauseButton.GetComponent<Image>().color = activeSpeedColor;
                normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
                fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;

            }
            else
            {
                heart.ResumeBeating();
                pauseButton.SetPausedText(false);
                pauseButton.GetComponent<Image>().color = inactiveSpeedColor;
                if(heart.GetGameSpeed()==GameSpeed.Fast)
                { 
                    normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
                fastForwardButton.GetComponent<Image>().color = activeSpeedColor;
                }
                else
                {
                    normalSpeedButton.GetComponent<Image>().color = activeSpeedColor;
                    fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;
                }
            }
        }

        public void TogglePause()
        {
          SetPausedState(!heart.IsPaused);
        }

        public void SetNormalSpeed()
        {
            if(heart.IsPaused)
                SetPausedState(false);
            heart.SetGameSpeed(GameSpeed.Normal);
            normalSpeedButton.GetComponent<Image>().color = activeSpeedColor;
            fastForwardButton.GetComponent<Image>().color = inactiveSpeedColor;


        }

        public void SetFastForward()
        {

            if (heart.IsPaused)
                SetPausedState(false);
            heart.SetGameSpeed(GameSpeed.Fast);

            normalSpeedButton.GetComponent<Image>().color = inactiveSpeedColor;
            fastForwardButton.GetComponent<Image>().color = activeSpeedColor;

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
            var stacks = tabletopContainer.GetElementStacksManager().GetStacks();
            foreach (var stack in stacks)
                if (slotSpec.GetSlotMatchForAspects(stack.GetAspects()).MatchType == SlotMatchForAspectsType.Okay)
                    return stack;

            return null;
        }

        public IEnumerable<ISituationAnchor> GetAllSituationTokens()
        {
            return tabletopContainer.GetAllSituationTokens();
        }

        public void ArrangeTokenOnTable(DraggableToken token)
        {
			token.transform.localPosition = GetFreeTokenPosition(token, new Vector2(0, -250f));
            tabletopContainer.PutOnTable(token);
        }

        //we place stacks horizontally rather than vertically
        public void ArrangeTokenOnTable(ElementStackToken stack)
        {
			stack.transform.localPosition = GetFreeTokenPosition(stack, new Vector2(-100f, 0f));
            tabletopContainer.PutOnTable(stack);
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
            foreach (var t in tabletopContainer.GetTokenTransformWrapper().GetTokens())
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

        void UpdateElementOverview() {
            // TODO: This does a lot of iterating each frame to grab all cards in play. If possible change this to use the planned "lists" instead
            // TODO: This is being called every frame in update, if possible only call it when the stacks have changed? Have a global "elements changed" event to call?

            var manager = tabletopContainer.GetElementStacksManager();
            var situations = tabletopContainer.GetAllSituationTokens();
            var draggedElementStack = (DraggableToken.itemBeingDragged != null ? DraggableToken.itemBeingDragged as ElementStackToken : null);
            int count;

            for (int i = 0; i < overviewElementIds.Length; i++) {
                count = manager.GetCurrentElementQuantity(overviewElementIds[i]);

                foreach (var sit in situations) 
                    count += sit.SituationController.GetElementCountInSituation(overviewElementIds[i]);

                if (draggedElementStack != null && draggedElementStack.Id == overviewElementIds[i])
                    count += draggedElementStack.Quantity;

                elementOverview.SetElement(i, overviewElementIds[i], count);
            }
        }

        void SetNextAnimTime() {
            nextAnimTime = Time.time + timeBetweenAnims - timeBetweenAnimsVariation + UnityEngine.Random.value * timeBetweenAnimsVariation * 2f;
        }

        void TriggerArtAnimation() {
            // TODO: This should randomly select a token to animate. Currently always picks health. Also only looks at tabletop, not all visible tokens.
            var manager = tabletopContainer.GetElementStacksManager();
            var stacks = manager.GetStacks();

            var animatableStacks = new List<IElementStack>();

            foreach (var stack in stacks) 
                if (stack.CanAnimate() && stack.Id != lastAnimID)
                    animatableStacks.Add(stack);

            if (animatableStacks.Count > 0) {
                int index = UnityEngine.Random.Range(0, animatableStacks.Count);

                animatableStacks[index].StartArtAnimation();
                lastAnimID = animatableStacks[index].Id;
            }
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
                tabletopContainer.GetTokenTransformWrapper().Accept(DraggableToken.itemBeingDragged);

                SoundManager.PlaySfx("CardDrop");
            }
        }

        void HandleOnBackgroundClicked()
        {
            //Close all open windows if we're not dragging (multi tap stuff)
            if (DraggableToken.itemBeingDragged == null)
                tabletopContainer.CloseAllSituationWindowsExcept(null);

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

            SetPausedState(true);
            var saveGameManager = new GameSaveManager(new GameDataImporter(compendium), new GameDataExporter());
            //try
            //{
                var htSave = saveGameManager.RetrieveHashedSaveFromFile();
                ClearGameState(heart,storage,tabletopContainer);
                saveGameManager.ImportHashedSaveToState(tabletopContainer,storage, htSave);
                StatusBar.UpdateCharacterDetailsView(storage);
                notifier.ShowNotificationWindow("Where were we?", " - we have loaded the game.");

            //}
            //catch (Exception e)
            //{
            //    notifier.ShowNotificationWindow("Couldn't load game - ", e.Message);
            //}
            SetPausedState(false);
            //heart.ResumeBeating();
        }

        public void SaveGame(bool withNotification)
        {
            heart.StopBeating();

            //Close all windows and dump tokens to desktop before saving.
            //We don't want or need to track half-started situations.
            var allSituationTokens = tabletopContainer.GetAllSituationTokens();
            foreach (var t in allSituationTokens)
                t.SituationController.CloseSituation();

            //try
            //{
                var saveGameManager =new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()),new GameDataExporter());
            saveGameManager.SaveActiveGame(tabletopContainer,Registry.Retrieve<Character>());
            if(withNotification)
                notifier.ShowNotificationWindow("SAVED THE GAME", "BUT NOT THE WORLD");

            //}
            //catch (Exception e)
            //{

            //    notifier.ShowNotificationWindow("Couldn't save game - ", e.Message); ;
            //}

            heart.ResumeBeating();
        }

        public void ShowDestinationsForStack(IElementStack stack)
        {
            var openToken = tabletopContainer.GetOpenToken();

            if (openToken !=null)
               openToken.ShowDestinationsForStack(stack);

            if (mapContainer != null)
                mapContainer.ShowDestinationsForStack(stack);
        }

        public void DecayStacksOnTable(float interval)
        {
            var decayingStacks = tabletopContainer.GetElementStacksManager().GetStacks().Where(s => s.Decays);
            foreach(var d in decayingStacks)
                d.Decay(interval);
        }

        private void HandleDragStateChanged(bool isDragging) {
            var draggedElement = DraggableToken.itemBeingDragged as ElementStackToken;
            
            // not dragging a stack? then do nothing. TabletopContainer was destroyed (end of game?)
            if (draggedElement == null || tabletopContainer == null)
                return;

            var tabletopStacks = tabletopContainer.GetElementStacksManager().GetStacks();
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
