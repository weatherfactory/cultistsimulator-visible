using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

// This is a "version" of the discussed BoardManager. Creates View Objects, Listens to their input.
namespace Assets.CS.TabletopUI
{
    public class TabletopManager : MonoBehaviour,ITokenSubscriber{

        [Header("Existing Objects")]
        [SerializeField] Transform tableLevel;
        [SerializeField] Transform windowLevel;
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

           tabletopObjectBuilder  = new TabletopObjectBuilder(tableLevel,windowLevel,GetStacksOnTabletopGateway());
            tabletopObjectBuilder.PopulateTabletop();
            var needsToken= tabletopObjectBuilder.BuildNewTokenRunningRecipe("needs");
            ArrangeTokenOnTable(needsToken);

        }

        public void ModifyElementQuantity(string elementId, int change)
        {
            if (Registry.Compendium.GetElementById(elementId) == null)
                Debug.Log("Can't find element with id " + elementId);
            else
            {
                IElementStacksWrapper wrapper = new TabletopElementStacksWrapper(tableLevel);
                ElementStacksGateway esg = new ElementStacksGateway(wrapper);
                if (change > 0)
                    esg.IncreaseElement(elementId, change);
                else
                    esg.ReduceElement(elementId, change);
            }
        }

        private ElementStacksGateway GetStacksOnTabletopGateway()
        {
            //NOTE: ultimately this should have an option to aggregate all stacks everywhere EXCEPT in situations
            //but INCLUDING stacks not added when this is created
            IElementStacksWrapper tabletopStacksWrapper = new TabletopElementStacksWrapper(tableLevel);
            return new ElementStacksGateway(tabletopStacksWrapper);
        }



        #region -- CREATE / REMOVE VIEW OBJECTS ----------------------------------------------------

        void ShowSituationWindow(SituationToken situationToken)
        {
            CloseAllSituationWindowsExcept(situationToken);
            PutTokenInAir(situationToken.transform as RectTransform);
            situationToken.Open();
   
        }

        private void CloseAllSituationWindowsExcept(SituationToken except)
        {
            var situationTokens = windowLevel.GetComponentsInChildren<SituationToken>().Where(sw => sw != except);
            foreach (var situationToken in situationTokens)
                HideSituationWindow(situationToken, false);

        }

        void HideSituationWindow(SituationToken situationToken, bool keepCards) {
            if (DraggableToken.itemBeingDragged  == null || DraggableToken.itemBeingDragged.gameObject != situationToken.gameObject)
                PutOnTable(situationToken.transform as RectTransform); // remove verb from details window before hiding it, so it isn't removed, if we're not already dragging it

            // Going through cards in slots
            var heldCards = situationToken.linkedWindow.GetComponentsInChildren<ElementStack>();

            foreach (var item in heldCards) {
                if (keepCards) // not completing the recipe= keep the cards. Ideally the recipe has already consumed the cards at this point, so we should always free what we have
                    PutOnTable(item.transform as RectTransform); // remove cards from details window before hiding it, so they aren't removed
            }
            situationToken.Close();
        }



        #endregion

        public void ArrangeTokenOnTable(DraggableToken token)
        {
            ///token.RectTransform.rect.Contains()... could iterate over and find overlaps
            token.transform.localPosition=new Vector3(-500,-250);
            PutOnTable(token.RectTransform);
        }

        public void PutOnTable(RectTransform rectTransform) {
            if (rectTransform == null)
                return;

            rectTransform.SetParent(tableLevel); 
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0f);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, rectTransform.eulerAngles.z);
        }

        // parents object to "TabletopTransform" and sets its Z to 0.
        public void PutTokenInAir(RectTransform rectTransform) {
            if (rectTransform == null)
                return;

            rectTransform.SetParent(tableLevel); 
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, windowZOffset);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, rectTransform.eulerAngles.z);
        }



        #region -- response to subscriptionUI events

        public void TokenEffectCommandSent(DraggableToken draggableToken, IEffectCommand effectCommand)
        {
            //disabled while we try affecting only local elements in a situation
            //foreach (var kvp in effectCommand.GetElementChanges())
            //{
            //    ModifyElementQuantity(kvp.Key,kvp.Value);
            //}
        }

        public void TokenPickedUp(DraggableToken draggableToken)
        {
            ElementStack cardPickedUp=draggableToken as ElementStack;
            if(cardPickedUp!=null)
            {
                if(cardPickedUp.Quantity>1)
                {
                var cardLeftBehind = PrefabFactory.CreateTokenWithSubscribers<ElementStack>(tableLevel);
                cardLeftBehind.transform.position = draggableToken.transform.position;
                cardLeftBehind.Populate(cardPickedUp.ElementId, cardPickedUp.Quantity-1);
                cardPickedUp.SetQuantity(1);
                }
            }
            
        }

        public void TokenInteracted(DraggableToken draggableToken)
        {
                SituationToken box = draggableToken as SituationToken;
            if (box != null)
            {
                if (!box.IsOpen)
                    ShowSituationWindow(box);
                else
                    HideSituationWindow(box, true);
            }

        }


        public void TokenReturnedToTabletop(DraggableToken draggableToken, INotification reason)
        {
            PutOnTable(draggableToken.RectTransform);
        }

        #endregion

        #region -- INTERACTION ----------------------------------------------------

        void HandleOnBackgroundDropped() {
            // NOTE: This puts items back on the background. We need this in more cases. Should be a method
            if (DraggableToken.itemBeingDragged != null) { // Maybe check for item type here via GetComponent<Something>() != null?
                DraggableToken.resetToStartPos = false; // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
                // This currently treats everything as a token, even dragged windows. Instead draggables should have a type that can be checked for when returning token to default layer?
                // Dragged windows should not change in height during/after dragging, since they float by default

                PutOnTable(DraggableToken.itemBeingDragged.transform as RectTransform); // Make sure to parent back to the tabletop
            }
        }

        void HandleOnBackgroundClicked() {
            //Close all open windows if we're not dragging (multi tap stuff)
            if (DraggableToken.itemBeingDragged == null)
                CloseAllSituationWindowsExcept(null);

        }


        #endregion

    }

}
