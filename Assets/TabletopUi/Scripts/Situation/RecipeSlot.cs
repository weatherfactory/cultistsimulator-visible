using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Assets.CS.TabletopUI {



    public class RecipeSlot : Sphere, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

        public override ContainerCategory ContainerCategory => ContainerCategory.Threshold;

        // DATA ACCESS

        public IList<RecipeSlot> childSlots { get; set; }
        public RecipeSlot ParentSlot { get; set; }
        public string AnimationTag { get; set; }

        // VISUAL ELEMENTS
        public RecipeSlotViz viz;

        public TextMeshProUGUI SlotLabel;
        public Graphic border;
        public GraphicFader slotGlow;
        public LayoutGroup slotIconHolder;

        public GameObject GreedyIcon;
        public GameObject ConsumingIcon;

        bool lastGlowState;

        public bool IsBeingAnimated { get; set; }

        public override bool AllowStackMerge { get { return false; } }

        private SpherePath _situationPath;

        public override bool AllowDrag {
            get {
                return !GoverningSlotSpecification.Greedy || IsBeingAnimated;
            }
        }

        public override bool IsGreedy
        {
            get { return GoverningSlotSpecification != null && GoverningSlotSpecification.Greedy; }
        }

        public bool IsConsuming
        {
            get { return GoverningSlotSpecification.Consumes; }
        }

        public enum SlotModifier { Locked, Ongoing, Greedy, Consuming };

        public RecipeSlot() {
            childSlots = new List<RecipeSlot>();
        }

        public void Start() {
            ShowGlow(false, false);
            Registry.Get<LocalNexus>().TokenInteractionEvent.AddListener(ReactToDraggedToken);
            _notifiersForContainer.Add(Registry.Get<INotifier>());
            
        }

        public void Initialise(SlotSpecification slotSpecification,SpherePath situationPath)
        {
            _situationPath = situationPath;
            GoverningSlotSpecification = slotSpecification;
            gameObject.name = GetPath().ToString();

            if (slotSpecification == null)
                return;

            SlotLabel.text = slotSpecification.Label;

            GreedyIcon.SetActive(slotSpecification.Greedy);
            ConsumingIcon.SetActive(slotSpecification.Consumes);

        }


        void ReactToDraggedToken(TokenInteractionEventArgs args)
        {

            if (args.TokenInteractionType == TokenInteractionType.BeginDrag)
            {

                var stack = args.Token as ElementStackToken;

                if (stack == null)
                    return;
                if (GetElementStackInSlot() != null)
                    return; // Slot is filled? Don't highlight it as interactive
                if (IsBeingAnimated)
                    return; // Slot is being animated? Don't highlight
                if (IsGreedy)
                    return; // Slot is greedy? It can never take anything.
                if (stack.EntityId == "dropzone")
                    return; // Dropzone can never be put in a slot

                if (GetMatchForStack(stack).MatchType == SlotMatchForAspectsType.Okay)
                    ShowGlow(true,false);
            }


            else if (args.TokenInteractionType == TokenInteractionType.EndDrag)
                    ShowGlow(false, false);


        }

        bool CanInteractWithDraggedObject(AbstractToken token) {
            if (lastGlowState == false || token == null)
                return false;

            var stack = token as ElementStackToken;

            if (stack == null)
                return false; // we only accept stacks

            //does the token match the slot? Check that first
            ContainerMatchForStack match = GetMatchForStack(stack);

            return match.MatchType == SlotMatchForAspectsType.Okay;
        }

        // IGlowableView implementation

        public virtual void OnPointerEnter(PointerEventData eventData) {
            if (GoverningSlotSpecification.Greedy) // never show glow for greedy slots
                return;

            //if we're not dragging anything, and the slot is empty, glow the slot.
            if (!eventData.dragging) {
                if (GetTokenInSlot() == null)
                    ShowHoverGlow(true);
            }
            else if (CanInteractWithDraggedObject(eventData.pointerDrag.GetComponent<AbstractToken>())) {
                if (lastGlowState)
                    eventData.pointerDrag.GetComponent<AbstractToken>().HighlightPotentialInteractionWithToken(true);

                if (GetTokenInSlot() == null) // Only glow if the slot is empty
                    ShowHoverGlow(true);
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
            if (GoverningSlotSpecification.Greedy) // we're greedy? No interaction.
                return;

            if(eventData.dragging)
            {
                var potentialDragToken = eventData.pointerDrag.GetComponent<AbstractToken>();

                if (lastGlowState && potentialDragToken != null)
                    potentialDragToken.HighlightPotentialInteractionWithToken(false);
            }
            ShowHoverGlow(false);
        }

        public void SetGlowColor(UIStyle.TokenGlowColor colorType) {
            SetGlowColor(UIStyle.GetGlowColor(colorType));
        }

        public void SetGlowColor(Color color) {
            slotGlow.SetColor(color);
        }

        public void ShowGlow(bool glowState, bool instant) {
            lastGlowState = glowState;

            if (glowState)
                slotGlow.Show(instant);
            else
                slotGlow.Hide(instant);
        }

        // Separate method from ShowGlow so we can restore the last state when unhovering
        protected virtual void ShowHoverGlow(bool show) {
            // We're NOT dragging something and our last state was not "this is a legal drop target" glow, then don't show
            //if (AbstractToken.itemBeingDragged == null && !lastGlowState)
            //    return;

            if (show) {
                SetGlowColor(UIStyle.TokenGlowColor.OnHover);
                SoundManager.PlaySfx("TokenHover");
                slotGlow.Show();
            }
            else {
                SetGlowColor(UIStyle.TokenGlowColor.Default);
                //SoundManager.PlaySfx("TokenHoverOff");

                if (lastGlowState)
                    slotGlow.Show();
                else
                    slotGlow.Hide();
            }
        }

        public bool HasChildSlots() {
            if (childSlots == null)
                return false;
            return childSlots.Count > 0;
        }




        public void OnDrop(PointerEventData eventData)
        {

            var stack = eventData.pointerDrag.GetComponent<ElementStackToken>();

            if (GoverningSlotSpecification.Greedy) // we're greedy? No interaction.
                return;

            if (IsBeingAnimated || !eventData.dragging || stack==null)
                return;

            TryAcceptStackAsThreshold(stack);

        }

        
        

        public override void DisplayHere(IToken token, Context context) {
            base.DisplayHere(token, context);
            var stack = token as ElementStackToken;

            if (stack != null) {
                slotIconHolder.transform.SetAsLastSibling();
            }
        }

        public AbstractToken GetTokenInSlot() {
            return GetComponentInChildren<AbstractToken>();
        }

        public ElementStackToken GetElementStackInSlot()
        {
            if (GetStacks().Count() > 1)
            {
                NoonUtility.Log("Something weird in slot " + GoverningSlotSpecification.Id +
                                ": it has more than one stack, so we're just returning the first.");
                return GetStacks().First();

            }

            return GetStacks().SingleOrDefault();
        }


        public override void TryMoveAsideFor(ElementStackToken potentialUsurper, AbstractToken incumbent, out bool incumbentMoved) {
            if (IsGreedy) { // We do not allow
                incumbentMoved = false;
                return;
            }

            //incomer is a token. Does it fit in the slot?
            if (GetMatchForStack(potentialUsurper).MatchType==SlotMatchForAspectsType.Okay && potentialUsurper.Quantity == 1)
            {
                incumbentMoved = true;
                incumbent.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag)); //do this first; AcceptStack will trigger an update on the displayed aspects
                AcceptStack(potentialUsurper, new Context(Context.ActionSource.PlayerDrag));
            }
            else
                incumbentMoved = false;
        }



        /// <summary>
        /// path to slot expressed in underscore-separated slot specification labels: eg "work_sacrifice"
        /// </summary>
        public override SpherePath GetPath()
        {

            SpherePath path;
            if (ParentSlot != null)
                path = new SpherePath(ParentSlot.GetPath(), GoverningSlotSpecification.Id);
            else
                path = new SpherePath(_situationPath,GoverningSlotSpecification.Id);


            if (!string.IsNullOrEmpty(_localPath))
                NoonUtility.Log($"We're trying to specify a spherepath ({_localPath}) in a recipe slot / threshold ({path})");
            return path;
        }

        public override void ActivatePreRecipeExecutionBehaviour() {
            if (GoverningSlotSpecification.Consumes) {
                var stack = GetElementStackInSlot();

                if (stack != null)
                    stack.MarkedForConsumption = true;
            }
        }

        public bool IsPrimarySlot()
        {
            return ParentSlot == null;
        }

        public void OnPointerClick(PointerEventData eventData) {
            bool highlightGreedy = GreedyIcon.gameObject.activeInHierarchy && eventData.hovered.Contains(GreedyIcon);
            bool highlightConsumes = ConsumingIcon.gameObject.activeInHierarchy && eventData.hovered.Contains(ConsumingIcon);

            Registry.Get<INotifier>().ShowSlotDetails(GoverningSlotSpecification, highlightGreedy, highlightConsumes);

        }

        }


    
}
