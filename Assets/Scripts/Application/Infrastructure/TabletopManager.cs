#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.Services;
using Assets.Logic;
using SecretHistories.Constants;
using SecretHistories.UI;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;

using SecretHistories.Enums.Elements;
using SecretHistories.Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

namespace SecretHistories.UI {
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
            var registry = new Watchman();
            registry.Register(this);
        }



        public async void EndGame(Ending ending, Token _anchor)
		{
			NoonUtility.Log("TabletopManager.EndGame()");

            
            var character = Watchman.Get<Character>();
            var chronicler = Watchman.Get<Chronicler>();

            chronicler.ChronicleGameEnd(Watchman.Get<SituationsCatalogue>().GetRegisteredSituations(), Watchman.Get<SphereCatalogue>().GetSpheres(),ending);
            character.Reset(null,ending);

            var saveTask = Watchman.Get<GameSaveManager>().SaveActiveGameAsync(new InactiveTableSaveState(Watchman.Get<MetaInfo>()), Watchman.Get<Character>(),SourceForGameState.DefaultSave);
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



		public void CloseAllDetailsWindows()
		{
			if (aspectDetailsWindow != null)
				aspectDetailsWindow.Hide();
			if (tokenDetailsWindow != null)
				tokenDetailsWindow.Hide();
		}

        public void CloseAllSituationWindowsExcept(string exceptVerbId) {
            var situations = Watchman.Get<SituationsCatalogue>().GetRegisteredSituations();

            foreach (var s in situations)
            {
                if (s.Verb.Id != exceptVerbId)
                    s.Close();
            }
        }

        public bool IsSituationWindowOpen() {
	        var situationControllers = Watchman.Get<SituationsCatalogue>().GetRegisteredSituations();
	        return situationControllers.Any(c => c.IsOpen);
        }

        public void SetHighlightedElement(string elementId, int quantity = 1)
        {
            var enableAccessibleCards =
                Watchman.Get<Config>().GetConfigValueAsInt(NoonConstants.ACCESSIBLECARDS);

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
            if(!currentDoomTokens.Contains(situationToken.Payload.Id))
                currentDoomTokens.Add(situationToken.Payload.Id);
            backgroundMusic.PlayImpendingDoom();
        }


        public void NoMoreImpendingDoom(Token situationToken)
        {
            if (currentDoomTokens.Contains(situationToken.Payload.Id))
                currentDoomTokens.Remove(situationToken.Payload.Id);
            if(!currentDoomTokens.Any())
                backgroundMusic.NoMoreImpendingDoom();
        }






    }


}
