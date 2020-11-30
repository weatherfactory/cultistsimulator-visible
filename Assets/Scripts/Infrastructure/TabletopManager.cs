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
using Assets.Scripts.Infrastructure;
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
    public class TabletopManager : MonoBehaviour
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
        [SerializeField] private ElementOverview _elementOverview;
        
        

        private bool _initialised;



        private static bool highContrastMode = false;
        private static bool accessibleCards = false;
        private List<string> currentDoomTokens = new List<string>();


        void Awake()
        {
            var registry = new Registry();
            registry.Register(this);
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



    }


}
