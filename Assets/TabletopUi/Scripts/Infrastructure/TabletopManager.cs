using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
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

// This is a "version" of the discussed BoardManager. Creates View Objects, Listens to their input.

namespace Assets.CS.TabletopUI
{
    public class TabletopManager : MonoBehaviour
    {

        [Header("Existing Objects")] [SerializeField] public TabletopContainer tabletopContainer;

        [SerializeField] private RectTransform draggableHolderRectTransform;
        [SerializeField] Transform windowLevel;
        [SerializeField] TabletopBackground background;
        [SerializeField] Transform windowHolderFixed;
        [SerializeField] private Heart heart;
        [SerializeField] private PauseButton pauseButton;
        [SerializeField] private Notifier notifier;
        private TabletopObjectBuilder tabletopObjectBuilder;
        [SerializeField] private RestartPanel restartPanel;
        [SerializeField] private OptionsPanel optionsPanel;
        [SerializeField] private RectTransform viewport;


        public void Update()
        {

            if (Input.GetKeyDown(KeyCode.Space))
                TogglePause();

            if (Input.GetKeyDown(KeyCode.Escape))
                optionsPanel.ToggleVisibility();
            //if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
            //{
            //    viewport.localPosition = new Vector3(viewport.localPosition.x,viewport.localPosition.y,viewport.localPosition.z+100);
            //}
            //if (Input.GetAxis("Mouse ScrollWheel") < 0) // forward
            //{
            //    viewport.localPosition = new Vector3(viewport.localPosition.x, viewport.localPosition.y, viewport.localPosition.z - 100);
            //}
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

            tabletopObjectBuilder = new TabletopObjectBuilder(tabletopContainer.transform);

       
            registry.Register<IDraggableHolder>(new DraggableHolder(draggableHolderRectTransform));
            registry.Register<IDice>(new Dice());
            registry.Register<TabletopManager>(this);
            registry.Register<TabletopObjectBuilder>(tabletopObjectBuilder);
            registry.Register<INotifier>(notifier);
            registry.Register<Character>(new Character());


            // Init Listeners to pre-existing Display Objects
            background.onDropped += HandleOnBackgroundDropped;
            background.onClicked += HandleOnBackgroundClicked;

            notifier.ShowNotificationWindow("18th JANUARY, 1920","I am a beginning student of the invisible arts. I have only time, hunger, and a little money. Earlier, I made a note in my journal. [Clicking the note, above, will read it.]",30);

            SetupBoard();

        }

        public void SetupBoard()
        {
            heart.StartBeating(0.05f);
            tabletopObjectBuilder.PopulateTabletop();
            AspectsDictionary startingElements = new AspectsDictionary
            {
                { "health", 2},
                { "reason", 2},
                { "passion", 2},
                { "shilling", 99},
                {"startingletter",1 }
            };

            foreach (var e in startingElements)
            {
                ElementStackToken token= tabletopContainer.GetTokenTransformWrapper().ProvisionElementStackAsToken(e.Key,e.Value);
                ArrangeTokenOnTable(token);
            }

            var needsSituationCreationCommand = new SituationCreationCommand(null, Registry.Retrieve<ICompendium>().GetRecipeById("startingneeds"));
            BeginNewSituation(needsSituationCreationCommand);
        }

        public void BeginNewSituation(SituationCreationCommand scc)
        {
            var token = tabletopObjectBuilder.BuildSituation(scc);

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


        public void ClearBoard()
        {
            foreach (var s in tabletopContainer.GetAllSituationTokens())
                s.Retire();

            foreach (var e in tabletopContainer.GetElementStacksManager().GetStacks())
                e.Retire(true); //looks daft but pretty on reset
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

        public void EndGame(Notification endGameNotification)
        {
            heart.StopBeating(); //note: not setting IsPaused, so can't resume with pause button. But this is a quick fix - we should disable or hide everything.
            restartPanel.Display(endGameNotification);

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

            heart.StopBeating();
            var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());
            try
            {
                var htSave = saveGameManager.RetrieveHashedSave("save.txt");
                ClearBoard();
                saveGameManager.ImportHashedSaveToContainer(tabletopContainer, htSave);
                notifier.ShowNotificationWindow("WE ARE WHAT WE WERE", " - we have loaded the game.");

           }
            catch (Exception e)
            {
                notifier.ShowNotificationWindow("Couldn't load game - ", e.Message);
            }
            heart.ResumeBeating();
        }

        public void SaveGame()
        {
            heart.StopBeating();

            try
            {
            var saveGameManager =new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()),new GameDataExporter());
            saveGameManager.SaveGame(tabletopContainer,"save.txt");
                notifier.ShowNotificationWindow("SAVED THE GAME", "BUT NOT THE WORLD");

            }
            catch (Exception e)
            {

                notifier.ShowNotificationWindow("Couldn't save game - ", e.Message); ;
            }

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
    }

}
