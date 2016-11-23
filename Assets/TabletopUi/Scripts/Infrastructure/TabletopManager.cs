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
    public class TabletopManager : MonoBehaviour{

        [Header("Existing Objects")]
        [SerializeField] public TabletopContainer tabletopContainer;
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

           tabletopObjectBuilder  = new TabletopObjectBuilder(tabletopContainer.transform,windowLevel);
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
                if (change > 0)
                    GetStacksOnTabletopGateway().IncreaseElement(elementId, change);
                else
                    GetStacksOnTabletopGateway().ReduceElement(elementId, change);
            }
        }

        private ElementStacksGateway GetStacksOnTabletopGateway()
        {
            IElementStacksWrapper tabletopStacksWrapper = new TabletopElementStacksWrapper(tabletopContainer.transform);
            return new ElementStacksGateway(tabletopStacksWrapper);
        }



        #region -- CREATE / REMOVE VIEW OBJECTS ----------------------------------------------------

        public void ShowSituationWindow(SituationToken situationToken)
        {
            CloseAllSituationWindowsExcept(situationToken);
            PutTokenInAir(situationToken.transform as RectTransform);
            situationToken.Open();
   
        }

        private void CloseAllSituationWindowsExcept(SituationToken except)
        {
            var situationTokens = windowLevel.GetComponentsInChildren<SituationToken>().Where(sw => sw != except);
            foreach (var situationToken in situationTokens)
                HideSituationWindow(situationToken);

        }

        public void HideSituationWindow(SituationToken situationToken) {
            if (DraggableToken.itemBeingDragged  == null || DraggableToken.itemBeingDragged.gameObject != situationToken.gameObject)
                PutOnTable(situationToken); // remove verb from details window before hiding it, so it isn't removed, if we're not already dragging it

            situationToken.Close();
        }



        #endregion

        public void ArrangeTokenOnTable(DraggableToken token)
        {
            ///token.RectTransform.rect.Contains()... could iterate over and find overlaps
            token.transform.localPosition=new Vector3(-500,-250);
            PutOnTable(token);
        }

        public void PutOnTable(DraggableToken token)
        {
            var stack = token as ElementStack;
            if(stack!=null)
                GetStacksOnTabletopGateway().AcceptStack(stack);

            var situation = token as SituationToken;
                if(token!=null)
                token.SetContainer(tabletopContainer);

            token.RectTransform.SetParent(tabletopContainer.transform); 
           token.RectTransform.anchoredPosition3D = new Vector3(token.RectTransform.anchoredPosition3D.x, token.RectTransform.anchoredPosition3D.y, 0f);
            token.RectTransform.localRotation = Quaternion.Euler(0f, 0f, token.RectTransform.eulerAngles.z);
        }

        // parents object to "TabletopTransform" and sets its Z to 0.
        public void PutTokenInAir(RectTransform rectTransform) {
            if (rectTransform == null)
                return;

            rectTransform.SetParent(windowLevel); 
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, windowZOffset);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, rectTransform.eulerAngles.z);
        }



        #region -- INTERACTION ----------------------------------------------------

        void HandleOnBackgroundDropped() {
            // NOTE: This puts items back on the background. We need this in more cases. Should be a method
            if (DraggableToken.itemBeingDragged != null) { // Maybe check for item type here via GetComponent<Something>() != null?
                DraggableToken.resetToStartPos = false; // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
                // This currently treats everything as a token, even dragged windows. Instead draggables should have a type that can be checked for when returning token to default layer?
                // Dragged windows should not change in height during/after dragging, since they float by default

                PutOnTable(DraggableToken.itemBeingDragged); // Make sure to parent back to the tabletop
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
