﻿using System;
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
using OrbCreationExtensions;
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

        [Header("Drag & Window")]
        [SerializeField] private RectTransform draggableHolderRectTransform;
        [SerializeField] Transform windowLevel;

        [Header("Options Bar & Notes")]
        [SerializeField] private PauseButton pauseButton;
        [SerializeField] private Notifier notifier;
        [SerializeField] private OptionsPanel optionsPanel;
        [SerializeField] private ElementOverview elementOverview;

        // A total number of 4 supported by the bar currently. Not more, not fewer
        private string[] overviewElementIds = new string[] {
            "health", "passion", "reason", "shilling"
        };

        public void Update()
        {
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
            registry.Register<ICompendium>(compendium);
            UpdateCompendium(compendium);
            SetNextAnimTime(); // sets the first animation for the tabletop Controller

            tabletopObjectBuilder = new TabletopObjectBuilder(tabletopContainer.transform, windowLevel);
         
            registry.Register<IDraggableHolder>(new DraggableHolder(draggableHolderRectTransform));
            registry.Register<IDice>(new Dice());
            registry.Register<TabletopManager>(this);
            registry.Register<TabletopObjectBuilder>(tabletopObjectBuilder);
            registry.Register<INotifier>(notifier);
            registry.Register<Character>(new Character());

            // Init Listeners to pre-existing Display Objects
            background.onDropped += HandleOnBackgroundDropped;
            background.onClicked += HandleOnBackgroundClicked;

            DraggableToken.onChangeDragState += HandleDragStateChanged;

            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());

            if (saveGameManager.DoesGameSaveExist() && saveGameManager.IsSavedGameActive())
                LoadGame();
            else
                SetupNewBoard();

           heart.StartBeating(0.05f);
        }

        private void OnDestroy() {
            // Sattic event so make sure to de-init once this object is destroyed
            DraggableToken.onChangeDragState -= HandleDragStateChanged;
        }

        public void SetupNewBoard()
        {
            
            tabletopObjectBuilder.CreateInitialTokensOnTabletop();
            ProvisionStartingElements(CrossSceneState.GetChosenLegacy());
            DealStartingDecks();
           // var startingNeedsSituationCreationCommand = new SituationCreationCommand(null, Registry.Retrieve<ICompendium>().GetRecipeById("startingneeds"),SituationState.FreshlyStarted);
            //BeginNewSituation(startingNeedsSituationCreationCommand);
            var chosenLegacy = CrossSceneState.GetChosenLegacy();
            if (chosenLegacy == null)
                throw new ApplicationException("No initial Legacy specified");

            notifier.ShowNotificationWindow(chosenLegacy.Label, chosenLegacy.StartDescription, 30);
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
                ElementStackToken token = tabletopContainer.GetTokenTransformWrapper().ProvisionElementStackAsToken(e.Key, e.Value);
                ArrangeTokenOnTable(token);
            }
        }

        public void BeginNewSituation(SituationCreationCommand scc)
        {
            var token = tabletopObjectBuilder.CreateTokenWithAttachedControllerAndSituation(scc);

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
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void SetPausedState(bool pause)
        {
            if (pause)
            {
                heart.StopBeating();
                pauseButton.SetPausedState(true);
            }
            else
            {
                heart.ResumeBeating();
                pauseButton.SetPausedState(false);
            }
        }

        public void TogglePause()
        {
          SetPausedState(!heart.IsPaused);
        }

        public void EndGame(Ending ending)
        {
            CrossSceneState.SetCurrentEnding(ending);
            var ls=new LegacySelector(Registry.Retrieve<ICompendium>());
            CrossSceneState.SetAvailableLegacies(ls.DetermineLegacies(ending, null));

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
                DraggableToken.resetToStartPos = false;
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

        public void LoadGame()
        {
            ICompendium compendium = Registry.Retrieve<ICompendium>();
            IGameEntityStorage storage = Registry.Retrieve<Character>();

            SetPausedState(true);
            var saveGameManager = new GameSaveManager(new GameDataImporter(compendium), new GameDataExporter());
            //try
            //{
                var htSave = saveGameManager.RetrieveHashedSave();
                ClearGameState(heart,storage,tabletopContainer);
            
                saveGameManager.ImportHashedSaveToState(tabletopContainer,storage, htSave);
                notifier.ShowNotificationWindow("WE ARE WHAT WE WERE", " - we have loaded the game.");

            //}
            //catch (Exception e)
            //{
            //    notifier.ShowNotificationWindow("Couldn't load game - ", e.Message);
            //}
            heart.ResumeBeating();
        }

        public void SaveGame()
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
            if(openToken!=null)
               openToken.ShowDestinationsForStack(stack);

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
    }

}
