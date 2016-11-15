using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

// This is a "version" of the discussed BoardManager. Creates View Objects, Listens to their input.
namespace Assets.CS.TabletopUI
{
    public class TabletopManager : MonoBehaviour,ITokenSubscriber,ISituationWindowSubscriber{

        [Header("Existing Objects")]
        [SerializeField] Transform tabletopTransform;
        [SerializeField] Transform windowParent;
        [SerializeField] Notifier notifier;
        [SerializeField] TabletopBackground background;
        [SerializeField] Transform windowHolderFixed;
        [SerializeField] Heart heart;
        private TabletopObjectBuilder tabletopObjectBuilder;



        [Header("View Settings")]
        [SerializeField] float windowZOffset = -10f;

        
        void Start () {
            var compendiumHolder = gameObject.AddComponent<Registry>();
            compendiumHolder.ImportContentToCompendium();

            heart.BeginHeartbeat(0.05f);

            // Init Listeners to pre-existing Display Objects
            background.onDropped += HandleOnBackgroundDropped;
            background.onClicked += HandleOnBackgroundClicked;

           tabletopObjectBuilder  = new TabletopObjectBuilder(tabletopTransform);
            tabletopObjectBuilder.PopulateTabletop();
            
        }

        public void ModifyElementQuantity(string elementId, int change)
        {
            if (Registry.Compendium.GetElementById(elementId) == null)
                Debug.Log("Can't find element with id " + elementId);
            else
            {
                IElementStacksWrapper wrapper = new TabletopElementStacksWrapper(tabletopTransform);
                ElementStacksGateway esg = new ElementStacksGateway(wrapper);
                if (change > 0)
                    esg.IncreaseElement(elementId, change);
                else
                    esg.ReduceElement(elementId, change);
            }
        }



        #region -- CREATE / REMOVE VIEW OBJECTS ----------------------------------------------------




        void ShowSituationWindow(VerbBox box) {
         if (maxNumSituationWindows > 0 && situationWindows.Count == maxNumSituationWindows) 
                HideSituationWindow(situationWindows[0].GetVerbBox(), true);

            PutTokenInAir(box.transform as RectTransform);

            var window = PrefabFactory.CreateSituationWindowWithSubscribers(windowParent);
            window.transform.position = box.transform.position;
            window.SetVerb(box);
            situationWindows.Add(window);
        }

        void HideSituationWindow(VerbBox box, bool keepCards) {
            if (DraggableToken.itemBeingDragged  == null || DraggableToken.itemBeingDragged.gameObject != box.gameObject)
                PutTokenOnTable(box.transform as RectTransform); // remove verb from details window before hiding it, so it isn't removed, if we're not already dragging it

            // Going through cards in slots
            var heldCards = box.detailsWindow.GetComponentsInChildren<ElementStack>();

            foreach (var item in heldCards) {
                if (keepCards) // not completing the recipe= keep the cards. Ideally the recipe has already consumed the cards at this point, so we should always free what we have
                    PutTokenOnTable(item.transform as RectTransform); // remove cards from details window before hiding it, so they aren't removed
            }

            situationWindows.Remove(box.detailsWindow);
            box.detailsWindow.Hide();
        }


        public int maxNumSituationWindows = 1;
        public int maxNumElementWindows = 1;
        List<SituationWindow> situationWindows = new List<SituationWindow>();



        #endregion

        #region -- MOVE / CHANGE VIEW OBJECTS ----------------------------------------------------

        // parents object to "TabletopTransform" and sets its Z to 0.
        public void PutTokenOnTable(RectTransform rectTransform) {
            if (rectTransform == null)
                return;

            rectTransform.SetParent(tabletopTransform); 
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0f);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, rectTransform.eulerAngles.z);
        }

        // parents object to "TabletopTransform" and sets its Z to 0.
        public void PutTokenInAir(RectTransform rectTransform) {
            if (rectTransform == null)
                return;

            rectTransform.SetParent(tabletopTransform); 
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, windowZOffset);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, rectTransform.eulerAngles.z);
        }

        #endregion


        #region -- response to subscriptionUI events

        public void TokenEffectCommandSent(DraggableToken draggableToken, IEffectCommand effectCommand)
        {
            foreach (var kvp in effectCommand.ElementChanges)
            {
                ModifyElementQuantity(kvp.Key,kvp.Value);
            }
        }

        public void TokenPickedUp(DraggableToken draggableToken)
        {
            ElementStack cardPickedUp=draggableToken as ElementStack;
            if(cardPickedUp!=null)
            {
                if(cardPickedUp.Quantity>1)
                {
                var cardLeftBehind = PrefabFactory.CreateTokenWithSubscribers<ElementStack>(tabletopTransform);
                cardLeftBehind.transform.position = draggableToken.transform.position;
                cardLeftBehind.Populate(cardPickedUp.ElementId, cardPickedUp.Quantity-1);
                cardPickedUp.SetQuantity(1);
                }
            }
            
        }

        public void TokenInteracted(DraggableToken draggableToken)
        {
                VerbBox box = draggableToken as VerbBox;
            if (box != null)
            {
                if (box.detailsWindow == null)
                    ShowSituationWindow(box);
                else
                {
                    HideSituationWindow(box, true);
                }
            }


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
                    HideSituationWindow(box, true);
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


        #endregion

        public void SituationBegins(VerbBox box)
        {
            HideSituationWindow(box, false);
        }

        public void SituationUpdated(VerbBox box)
        {
            Debug.Log("Situation continues");
        }

        public void ElementStackRejected(ElementStack stack)
        {
            PutTokenOnTable(stack.RectTransform);
        }
    }

}
