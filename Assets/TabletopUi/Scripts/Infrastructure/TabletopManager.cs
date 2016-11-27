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

        [SerializeField] private RectTransform draggableHolder;
        [SerializeField] Transform windowLevel;
        [SerializeField] TabletopBackground background;
        [SerializeField] Transform windowHolderFixed;
        [SerializeField] Heart heart;
        private TabletopObjectBuilder tabletopObjectBuilder;

        [Header("View Settings")]
        [SerializeField] float windowZOffset = -10f;

   

        void Start () {
            var registry = gameObject.AddComponent<Registry>();

            var compendium= new Compendium();
            var contentImporter = new ContentImporter();
            contentImporter.PopulateCompendium(compendium);

            registry.RegisterCompendium(compendium);
            registry.RegisterDraggableHolder(draggableHolder);
            registry.RegisterDice(new Dice());
            registry.RegisterTabletopManager(this);

            heart.StartBeating(0.05f);

            // Init Listeners to pre-existing Display Objects
            background.onDropped += HandleOnBackgroundDropped;
            background.onClicked += HandleOnBackgroundClicked;

           tabletopObjectBuilder  = new TabletopObjectBuilder(tabletopContainer.transform,windowLevel);
            tabletopObjectBuilder.PopulateTabletop();
            var needsToken= tabletopObjectBuilder.BuildNewTokenRunningRecipe("needs");
            PlaceTokenOnTable(needsToken);

        }

        public HashSet<IRecipeSlot> FindStacksToFillSlots(HashSet<IRecipeSlot> slotsToFill)
        {
            var unprocessedSlots = new HashSet<IRecipeSlot>();
            foreach (var slot in slotsToFill)
            {
                if (!slot.Equals(null)) //it hasn't been destroyed
                {
                    if (slot.GetElementStackInSlot() == null)
                    {
                        var stack = findStackForSlotSpecification(slot.GoverningSlotSpecification);
                        if (stack != null)
                        {
                            stack.SplitAllButNCardsToNewStack(1);
                            slot.AcceptStack(stack);
                        }
                        else
                            unprocessedSlots.Add(slot);
                    }
                }
            }
            return unprocessedSlots;
        }


        private IElementStack findStackForSlotSpecification(SlotSpecification slotSpec)
        {
            var stacks= tabletopContainer.GetElementStacksManager().GetStacks();
                foreach(var stack in stacks)
                    if (slotSpec.GetSlotMatchForAspects(stack.GetAspects()).MatchType == SlotMatchForAspectsType.Okay)
                        return stack;

            return null;
        }
        

        public void PlaceTokenOnTable(DraggableToken token)
        {
            ///token.RectTransform.rect.Contains()... could iterate over and find overlaps
            token.transform.localPosition=new Vector3(-500,-250);
            tabletopContainer.PutOnTable(token);
        }


        public void CloseAllSituationWindowsExcept(SituationToken except)
        {
            var situationTokens = tabletopContainer.GetTokenTransformWrapper().GetSituationTokens().Where(sw => sw != except);
            foreach (var situationToken in situationTokens)
            {
                if (DraggableToken.itemBeingDragged == null ||
                    DraggableToken.itemBeingDragged.gameObject != situationToken.gameObject)

                    situationToken.CloseSituation();
            }
        }

        void HandleOnBackgroundDropped() {
            // NOTE: This puts items back on the background. We need this in more cases. Should be a method
            if (DraggableToken.itemBeingDragged != null) { // Maybe check for item type here via GetComponent<Something>() != null?
                DraggableToken.resetToStartPos = false; // This tells the draggable to not reset its pos "onEndDrag", since we do that here.
                // This currently treats everything as a token, even dragged windows. Instead draggables should have a type that can be checked for when returning token to default layer?
                // Dragged windows should not change in height during/after dragging, since they float by default

                //tabletopContainer.PutOnTable(DraggableToken.itemBeingDragged); // Make sure to parent back to the tabletop
                DraggableToken.itemBeingDragged.DisplayOnTable();
                tabletopContainer.GetTokenTransformWrapper().Accept(DraggableToken.itemBeingDragged);
            }
        }

        void HandleOnBackgroundClicked() {
            //Close all open windows if we're not dragging (multi tap stuff)
            if (DraggableToken.itemBeingDragged == null)
                CloseAllSituationWindowsExcept(null);

        }


        

    }

}
