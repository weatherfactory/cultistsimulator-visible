using System.Collections.Generic;
using UnityEngine;

// This is a "version" of the discussed BoardManager. Creates View Objects, Listens to their input.
namespace Assets.CS.TabletopUI
{
    public class TabletopManager : MonoBehaviour {

        [Header("Existing Objects")]
        [SerializeField] Transform cardHolder;
        [SerializeField] Transform windowParent;
        [SerializeField] Notifier notifier;
        [SerializeField] TabletopBackground background;

        [Header("Prefabs")]
        [SerializeField] ElementCardClickable elementCardPrefab;
        [SerializeField] VerbBox verbBoxPrefab;
        [SerializeField] RecipeDetailsWindow recipeDetailsWindowPrefab;
        
        [SerializeField] Transform windowHolderFixed;



  [Header("View Settings")]
        [SerializeField] float windowZOffset = -10f;

        
        void Start () {
            var compendiumHolder = gameObject.AddComponent<CompendiumHolder>();
            compendiumHolder.Init();
        


            // Init Listeners to pre-existing Display Objects
            background.onDropped += HandleOnBackgroundDropped;
            background.onClicked += HandleOnBackgroundClicked;
            Draggable.onChangeDragState += OnChangeDragState;

            PopulateTabletop();
        }


        void PopulateTabletop() {

            VerbBox box;
            ElementCardClickable card;

            float boxWidth = (verbBoxPrefab.transform as RectTransform).rect.width + 20f;
            float boxHeight = (verbBoxPrefab.transform as RectTransform).rect.height + 50f;
            float cardWidth = (elementCardPrefab.transform as RectTransform).rect.width + 20f;

            // build verbs
            var verbs = CompendiumHolder.compendium.GetAllVerbs();

            for (int i = 0; i < verbs.Count; i++) {
                box = BuildVerbBox();
                box.SetVerb(verbs[i]);
                box.transform.localPosition = new Vector3(-1000f + i * boxWidth, boxHeight);
            }


            for (int i = 0; i < 10; i++) {
                card = BuildElementCard();
                card.SetElement(legalElementIDs[i % legalElementIDs.Length], 1);
                card.transform.localPosition = new Vector3(-1000f + i * cardWidth, 0f);
            }
        }

        string[] legalElementIDs = new string[7] { 
            "health",  
            "reason",  
            "clique",  
            "ordinarylife",
            "suitablepremises",
            "occultscrap",
            "shilling"
        };

        #region -- CREATE / REMOVE VIEW OBJECTS ----------------------------------------------------

        // Verb Boxes

        // Ideally we pool and reuse these
        VerbBox BuildVerbBox() {
            var box = Instantiate(verbBoxPrefab) as VerbBox;
            box.onVerbBoxClicked += HandleOnVerbBoxClicked;
            box.transform.SetParent(cardHolder);
            box.transform.localScale = Vector3.one;
            box.transform.localPosition = Vector3.zero;
            box.transform.localRotation = Quaternion.identity;
            return box;
        }

        // Element Cards

        // Ideally we pool and reuse these
        ElementCardClickable BuildElementCard() {
            var card = Instantiate(elementCardPrefab) as ElementCardClickable;
            card.onCardClicked += HandleOnElementCardClicked;
            card.transform.SetParent(cardHolder);
            card.transform.localScale = Vector3.one;
            card.transform.localPosition = Vector3.zero;
            card.transform.localRotation = Quaternion.identity;
            card.Subscribe(notifier);
            return card;
        }

        // Element Detail Windows



        void ShowElementDetails(ElementCard card)
        {

            notifier.ShowElementDetails(card);
        }




        // Ideally we pool and reuse these

        // Recipe Detail Windows

        void ShowRecipeDetails(VerbBox box) {
         if (maxNumRecipeWindows > 0 && recipeWindows.Count == maxNumRecipeWindows) 
                HideRecipeDetails(recipeWindows[0].GetVerb(), true);

            PutTokenInAir(box.transform as RectTransform);

            var window = BuildRecipeDetailsWindow();
            window.transform.position = box.transform.position;
            window.SetVerb(box);
            recipeWindows.Add(window);
        }

        void HideRecipeDetails(VerbBox box, bool keepCards) {
            if (Draggable.itemBeingDragged  == null || Draggable.itemBeingDragged.gameObject != box.gameObject)
                PutTokenOnTable(box.transform as RectTransform); // remove verb from details window before hiding it, so it isn't removed, if we're not already dragging it

            // Going throug hte cards in the slots
            var heldCards = box.detailsWindow.GetAllHeldCards();

            foreach (var item in heldCards) {
                if (keepCards) // not completing the recipe= keep the cards. Ideally the recipe has already consumed the cards at this point, so we should always free what we have
                    PutTokenOnTable(item.transform as RectTransform); // remove cards from details window before hiding it, so they aren't removed
                else if (item.detailsWindow != null)
                    notifier.HideElementDetails(item);
            }

            recipeWindows.Remove(box.detailsWindow);
            box.detailsWindow.Hide();
        }

        // Ideally we pool and reuse these
        RecipeDetailsWindow BuildRecipeDetailsWindow() {
            var window = Instantiate(recipeDetailsWindowPrefab) as RecipeDetailsWindow;
            window.transform.SetParent(windowParent);
            window.transform.localPosition = Vector3.zero;
            window.transform.localScale = Vector3.one;
            window.transform.localRotation = Quaternion.identity;
            window.onStartRecipe += HandleOnRecipeStarted;
            return window;
        }

        
        public int maxNumRecipeWindows = 1;
        public int maxNumElementWindows = 1;
        List<RecipeDetailsWindow> recipeWindows = new List<RecipeDetailsWindow>();




        #endregion

        #region -- MOVE / CHANGE VIEW OBJECTS ----------------------------------------------------

        // parents object to "CardHolder" (should rename to TokenHolder) and sets it's Z to 0.
        public void PutTokenOnTable(RectTransform rectTransform) {
            if (rectTransform == null)
                return;

            rectTransform.SetParent(cardHolder); 
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0f);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, rectTransform.eulerAngles.z);
        }

        // parents object to "CardHolder" (should rename to TokenHolder) and sets it's Z to 0.
        public void PutTokenInAir(RectTransform rectTransform) {
            if (rectTransform == null)
                return;

            rectTransform.SetParent(cardHolder); 
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, windowZOffset);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, rectTransform.eulerAngles.z);
        }

        #endregion

        #region -- INTERACTION ----------------------------------------------------

        // This checks if we're dragging something and if we have to do some changes
        // Currently this closes the windows when dragging is being done
        // This was in the windows previously, but since i'm not holding a reference to 
        // all open windows here it had to move up.
        void OnChangeDragState (bool isDragging) {
            if (isDragging == false || Draggable.itemBeingDragged.gameObject == null) 
                return;

            ElementCard card = Draggable.itemBeingDragged.GetComponent<ElementCard>();

            if (card != null) {
                ShowElementDetails(card);			
                return;
            }

            VerbBox box = Draggable.itemBeingDragged.GetComponent<VerbBox>();

            if (box != null) {
                if (box.detailsWindow != null)
                    HideRecipeDetails(box, true);
                else
                    return;			
            }
        }

        void HandleOnBackgroundDropped() {
            // NOTE: This puts items back on the background. We need this in more cases. Should be a method
            if (Draggable.itemBeingDragged != null) { // Maybe check for item type here via GetComponent<Something>() != null?
                Draggable.resetToStartPos = false; // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
                // This currently treats everything as a token, even dragged windows. Instead draggables should have a type that can be checked for when returning token to default layer?
                // Dragged windows should not change in height during/after dragging, since they float by default

                PutTokenOnTable(Draggable.itemBeingDragged.transform as RectTransform); // Make sure to parent back to the tabletop
            }
        }

        void HandleOnBackgroundClicked() {
            // Close all open windows if we're not dragging (multi tap stuff)
            if (Draggable.itemBeingDragged == null)
                notifier.HideAllElementDetails();
 
        }

        void HandleOnElementCardClicked(ElementCard card) {
            if (card.detailsWindow == null)
                ShowElementDetails(card);
            else {
               notifier.HideElementDetails(card);
            }
        }

        void HandleOnVerbBoxClicked(VerbBox box) {
            if (box.detailsWindow == null)
                ShowRecipeDetails(box);
            else {
                HideRecipeDetails(box, true);
            }
        }

        void HandleOnRecipeStarted(RecipeDetailsWindow window, VerbBox box) {
            HideRecipeDetails(box, false);
            box.StartTimer();
        }

        #endregion
    }
}
