using System.Collections.Generic;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

// This is a "version" of the discussed BoardManager. Creates View Objects, Listens to their input.
namespace Assets.CS.TabletopUI
{
    public class TabletopManager : MonoBehaviour,ITokenSubscriber {

        [Header("Existing Objects")]
        [SerializeField] Transform cardHolder;
        [SerializeField] Transform windowParent;
        [SerializeField] Notifier notifier;
        [SerializeField] TabletopBackground background;
        [SerializeField] Transform windowHolderFixed;



  [Header("View Settings")]
        [SerializeField] float windowZOffset = -10f;

        
        void Start () {
            var compendiumHolder = gameObject.AddComponent<Registry>();
            compendiumHolder.ImportContentToCompendium();

            // Init Listeners to pre-existing Display Objects
            background.onDropped += HandleOnBackgroundDropped;
            background.onClicked += HandleOnBackgroundClicked;
            DraggableToken.onChangeDragState += OnChangeDragState;

            PopulateTabletop();

        }

        public IEnumerable<ElementStack> GetCardsOnTabletop()
        {
            return cardHolder.GetComponentsInChildren<ElementStack>();
        }


        void PopulateTabletop() {

            VerbBox box;
            ElementStack stack;

            float boxWidth = (PrefabFactory.GetPrefab<VerbBox>().transform as RectTransform).rect.width + 20f;
            float boxHeight = (PrefabFactory.GetPrefab<VerbBox>().transform as RectTransform).rect.height + 50f;
            float cardWidth = (PrefabFactory.GetPrefab<ElementStack>().transform as RectTransform).rect.width + 20f;


            // build verbs
            var verbs = Registry.compendium.GetAllVerbs();

            for (int i = 0; i < verbs.Count; i++) {
                box = BuildVerbBox();
                box.SetVerb(verbs[i]);
                box.transform.localPosition = new Vector3(-1000f + i * boxWidth, boxHeight);
            }


            for (int i = 0; i < 10; i++) {
                stack = BuildElementCard();
                stack.SetElement(legalElementIDs[i % legalElementIDs.Length], 3);
                stack.transform.localPosition = new Vector3(-1000f + i * cardWidth, 0f);
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
        VerbBox BuildVerbBox()
        {
            var box= PrefabFactory.CreateLocally<VerbBox>(cardHolder);
            box.onVerbBoxClicked += HandleOnVerbBoxClicked;
            return box;
        }

        // Element Cards

        // Ideally we pool and reuse these
        ElementStack BuildElementCard()
        {
            var card = PrefabFactory.CreateLocally<ElementStack>(cardHolder);
            card.Subscribe(this);
            card.Subscribe(notifier);
            return card;
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
            if (DraggableToken.itemBeingDragged  == null || DraggableToken.itemBeingDragged.gameObject != box.gameObject)
                PutTokenOnTable(box.transform as RectTransform); // remove verb from details window before hiding it, so it isn't removed, if we're not already dragging it

            // Going throug hte cards in the slots
            var heldCards = box.detailsWindow.GetComponentsInChildren<ElementStack>();

            foreach (var item in heldCards) {
                if (keepCards) // not completing the recipe= keep the cards. Ideally the recipe has already consumed the cards at this point, so we should always free what we have
                    PutTokenOnTable(item.transform as RectTransform); // remove cards from details window before hiding it, so they aren't removed
            }

            recipeWindows.Remove(box.detailsWindow);
            box.detailsWindow.Hide();
        }

        // Ideally we pool and reuse these
        SituationWindow BuildRecipeDetailsWindow()
        {
            var window = PrefabFactory.CreateLocally<SituationWindow>(windowParent);
            window.onStartRecipe += HandleOnRecipeStarted;
            return window;
        }

        public int maxNumRecipeWindows = 1;
        public int maxNumElementWindows = 1;
        List<SituationWindow> recipeWindows = new List<SituationWindow>();




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


        #region -- response to subscriptionUI events
        public void TokenPickedUp(DraggableToken draggableToken)
        {
            ElementStack cardPickedUp=draggableToken as ElementStack;
            if(cardPickedUp!=null)
            {
                if(cardPickedUp.Quantity>1)
                { 
                var card = BuildElementCard();
                card.transform.localPosition = draggableToken.transform.localPosition;
                card.SetElement(cardPickedUp.ElementId, cardPickedUp.Quantity-1);
                    cardPickedUp.SetQuantity(1);
                }
            }
        }

        public void TokenInteracted(DraggableToken draggableToken)
        {
       
        }

        #endregion

        #region -- INTERACTION ----------------------------------------------------

        // This checks if we're dragging something and if we have to do some changes
        // Currently this closes the windows when dragging is being done
        // This was in the windows previously, but since i'm not holding a reference to 
        // all open windows here it had to move up.
        void OnChangeDragState (bool isDragging) {
            if (isDragging == false || DraggableToken.itemBeingDragged.gameObject == null) 
                return;

            ElementStack stack = DraggableToken.itemBeingDragged.GetComponent<ElementStack>();

            if (stack != null) {
                notifier.ShowElementDetails(stack);			
                return;
            }

            VerbBox box = DraggableToken.itemBeingDragged.GetComponent<VerbBox>();

            if (box != null) {
                if (box.detailsWindow != null)
                    HideRecipeDetails(box, true);
                else
                    return;			
            }
        }

        void HandleOnBackgroundDropped() {
            // NOTE: This puts items back on the background. We need this in more cases. Should be a method
            if (DraggableToken.itemBeingDragged != null) { // Maybe check for item type here via GetComponent<Something>() != null?
                DraggableToken.resetToStartPos = false; // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
                // This currently treats everything as a token, even dragged windows. Instead draggables should have a type that can be checked for when returning token to default layer?
                // Dragged windows should not change in height during/after dragging, since they float by default

                PutTokenOnTable(DraggableToken.itemBeingDragged.transform as RectTransform); // Make sure to parent back to the tabletop
            }
        }

        void HandleOnBackgroundClicked() {
            //TODO: Close all open windows if we're not dragging (multi tap stuff)
 
        }



        void HandleOnVerbBoxClicked(VerbBox box) {
            if (box.detailsWindow == null)
                ShowRecipeDetails(box);
            else {
                HideRecipeDetails(box, true);
            }
        }

        void HandleOnRecipeStarted(SituationWindow window, VerbBox box) {
            HideRecipeDetails(box, false);
            box.StartTimer();
        }

        #endregion


    }

}
