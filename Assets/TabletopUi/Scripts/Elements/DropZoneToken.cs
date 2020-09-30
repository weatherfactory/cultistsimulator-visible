#pragma warning disable 0649
using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Logic;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace Assets.CS.TabletopUI {
    public class DropZoneToken : ElementStackToken, IElementStack, IGlowableView
    {

        protected override void Awake()
		{
            base.Awake();
        }

        private void InitialiseIfStackIsNew()
        {
            //these things should only be initialised if we've just created the stack
            //if we're repopulating, they'll already exist
/*
            if (_currentMutations == null)
                _currentMutations = new Dictionary<string, int>();
            if (_illuminateLibrarian == null)
                _illuminateLibrarian = new IlluminateLibrarian();
*/
            if (CurrentStacksManager == null)
                CurrentStacksManager = Registry.Get<Limbo>().GetElementStacksManager(); //a stack must always have a parent stacks manager, or we get a null reference exception
            //when first created, it should be in Limbo
        }

        public override string EntityId
		{
            get { return "dropzone"; }
        }

        public override string Label
        {
            get { return ""; }
        }

        public override string Icon
        {
            get { return ""; }
        }

		public override bool Unique
        {
            get
            {
                return true;
            }
        }

		public override string UniquenessGroup
        {
            get { return ""; }
        }

        public override bool Decays
		{
            get { return false; }
        }

        public override int Quantity
		{
            get { return 1; }
        }

        public override bool MarkedForConsumption { get { return false; } set {} }

        public override IlluminateLibrarian IlluminateLibrarian
        {
            get { return new IlluminateLibrarian(); }
            set {}
        }

		public override Dictionary<string, int> GetCurrentMutations()
        {
            return new Dictionary<string, int>();
        }
        public override Dictionary<string, string> GetCurrentIlluminations()
        {
            return new Dictionary<string, string>();
        }

        public override void SetMutation(string aspectId, int value,bool additive)
        {
        }

        public override Dictionary<string, List<MorphDetails>> GetXTriggers()
		{
            return new Dictionary<string, List<MorphDetails>>();
        }

        public override IAspectsDictionary GetAspects(bool includeSelf = true)
        {
            IAspectsDictionary aspectsToReturn=new AspectsDictionary();
            return aspectsToReturn;
        }

        public override List<SlotSpecification> GetChildSlotSpecificationsForVerb(string forVerb)
		{
            return new List<SlotSpecification>();
        }

        public override bool HasChildSlotsForVerb(string verb)
		{
            return false;
        }

        /// <summary>
        /// This is uses both for population and for repopulation - eg when an xtrigger transforms a stack
        /// Note that it (intentionally) resets the timer.
        /// </summary>
        public void Populate()
		{
            InitialiseIfStackIsNew();
			ShowGlow(false, false);
        }

        public override void ReturnToTabletop(Context context)
		{
            Registry.Get<Choreographer>().ArrangeTokenOnTable(this, context, lastTablePos, false);	// Never push other cards aside - CP
        }

        private bool IsOnTabletop()
		{
            return transform.parent.GetComponent<TabletopTokenContainer>() != null;
        }

        // Called from TokenContainer, usually after StacksManager told it to
        public override void SetTokenContainer(ITokenContainer newTokenContainer, Context context)
		{
            OldTokenContainer = TokenContainer;

            if (OldTokenContainer != null && OldTokenContainer != newTokenContainer)
                OldTokenContainer.SignalStackRemoved(this, context);

            TokenContainer = newTokenContainer;

            if (newTokenContainer != null)
                newTokenContainer.SignalStackAdded(this, context);
        }

        override public bool AllowsIncomingMerge()
		{
			return false;
        }

        override public bool AllowsOutgoingMerge()
		{
			return false;
        }

        #region -- Interaction ------------------------------------------------------------------------------------

		private List<TabletopUi.TokenAndSlot> FindValidSlot( IList<RecipeSlot> slots, TabletopUi.SituationController situation )
		{
			List<TabletopUi.TokenAndSlot> results = new List<TabletopUi.TokenAndSlot>();
			return results;
		}

		private void SendStackToNearestValidSlot()
		{
		}

        public override void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.clickCount > 1)
			{
				// Double-click, so abort any pending single-clicks
				singleClickPending = false;
				Registry.Get<Notifier>().HideDetails();
				SendStackToNearestValidSlot();
			}
			else
			{
				// Single-click BUT might be first half of a double-click
				// Most of these functions are OK to fire instantly - just the ShowCardDetails we want to wait and confirm it's not a double
				singleClickPending = true;

				//notifier.ShowCardElementDetails(this._element, this);

				// this moves the clicked sibling on top of any other nearby cards.
				// NOTE: We shouldn't do this if we're in a RecipeSlot.
				if (TokenContainer.GetType() != typeof(RecipeSlot))
					transform.SetAsLastSibling();
			}
        }

        public override void OnDrop(PointerEventData eventData)
		{
//            if (DraggableToken.itemBeingDragged != null)
//                DraggableToken.itemBeingDragged.InteractWithTokenDroppedOn(this);
        }

        public override bool CanInteractWithTokenDroppedOn(IElementStack stackDroppedOn)
		{
            return false;
        }

        public override void InteractWithTokenDroppedOn(IElementStack stackDroppedOn)
		{
        }

        void ShowNoMergeMessage(IElementStack stackDroppedOn)
		{
        }

        public override bool CanInteractWithTokenDroppedOn(SituationToken tokenDroppedOn)
		{
            return false;
        }

        public override void InteractWithTokenDroppedOn(SituationToken tokenDroppedOn)
		{
			// We can't interact? Then dump us on the tabletop
			DraggableToken.SetReturn(false, "Tried to drop on non-compatible token, return to tabletop");
			ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
        }

        #endregion

        override public void Flip(bool state, bool instant = false)
		{
        }

    }
}
