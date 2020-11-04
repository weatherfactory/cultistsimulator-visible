#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.SlotsContainers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.NullObjects;
using Assets.Core.Services;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Noon;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets.CS.TabletopUI {

    [Serializable]
    public class OnContainerAddedEvent : UnityEvent<TokenContainer> { }

    [Serializable]
    public class OnContainerRemovedEvent : UnityEvent<TokenContainer> { }


    [RequireComponent(typeof(SituationWindowPositioner))]
    public class SituationWindow : AbstractToken,ISituationSubscriber {

        [Header("Visuals")]
		[SerializeField] CanvasGroupFader canvasGroupFader;
        public SituationWindowPositioner positioner;

        [Space]
        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI title;
		public PaginatedText PaginatedNotes;

		[Space]
        [SerializeField] StartingSlotsManager startingSlots;

        [Space]
        [SerializeField] OngoingDisplay ongoingDisplay;

        [Space]
        [SerializeField] SituationResults results;
		[SerializeField] Button dumpResultsButton;
        [SerializeField] TextMeshProUGUI dumpResultsButtonText;

        [Space]
        [SerializeField] SituationStorage storage;

        [Space]
        [SerializeField] AspectsDisplay aspectsDisplay;

		[SerializeField] Button startButton;
		[SerializeField] TextMeshProUGUI startButtonText;

        public UnityEvent OnStart;
        public UnityEvent OnCollect;
        public UnityEvent OnWindowClosed;
        public OnContainerAddedEvent OnContainerAdded;
        public OnContainerRemovedEvent OnContainerRemoved;

        public TokenLocation LastOpenLocation;

        private IVerb Verb;
        private string _situationPath;
        private bool windowIsWide = false;

        public bool IsVisible {
            get { return canvasGroupFader.IsVisible(); }
        }

		public string Title {
			get { return title.text; }
			set { title.text = value; }
        }

		public Vector3 Position
		{
			get { return positioner.GetPosition(); }
			set { positioner.SetPosition( value ); }
		}
        // INIT & LIFECYCLE



        public void TryResizeWindow(int slotsCount)
        {
            SetWindowSize(slotsCount > 3);
        }
        public void DisplayIcon(string icon)
        {
            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(icon);
            artwork.sprite = sprite;
        }
         
        public void Populate(SituationCreationCommand situationCreationCommand) {
			Verb = situationCreationCommand.Verb;
            _situationPath = situationCreationCommand.SituationPath;
            name = "Window_" + Verb.Id;
            DisplayIcon(Verb.Id);

            Title = Verb.Label;
            PaginatedNotes.SetText(Verb.Description);
            startButton.onClick.AddListener(OnStart.Invoke);

            startingSlots.Initialise(Verb, this,_situationPath);
            ongoingDisplay.Initialise(OnContainerAdded, OnContainerRemoved,situationCreationCommand);
            results.Initialise();

            //this is an improvement - the situation doesn't need to know what to add - but better yet would be to tie together creation + container add, at runtime
            foreach (var s in startingSlots.GetAllSlots())
                OnContainerAdded.Invoke(s);

            OnContainerAdded.Invoke(storage);
            OnContainerAdded.Invoke(results);
            
            if (Verb.Startable)
            {
                startButton.gameObject.SetActive(true);
            }
            else
            {
                startButton.gameObject.SetActive(false);
            }

		}

        public override void StartArtAnimation()
        {
       
        }

        public override bool CanAnimateArt()
        {
            return false;
        }

        public override string EntityId { get; }

        public override void OnDrop(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        public override bool CanInteractWithIncomingObject(VerbAnchor tokenDroppedOn)
        {
            throw new NotImplementedException();
        }

        public override bool CanInteractWithIncomingObject(ElementStackToken stackDroppedOn)
        {
            throw new NotImplementedException();
        }

        public override void InteractWithIncomingObject(VerbAnchor tokenDroppedOn)
        {
            throw new NotImplementedException();
        }

        public override void InteractWithIncomingObject(ElementStackToken incomingStack)
        {
            throw new NotImplementedException();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
//
        }

        public override void ReturnToTabletop(Context context)
        {
            throw new NotImplementedException();
        }

        protected override void NotifyChroniclerPlacedOnTabletop()
        {
            throw new NotImplementedException();
        }

        public override bool Retire()
        {
            Destroy(gameObject);
            return true;
        }

        public override void ReactToDraggedToken(TokenInteractionEventArgs args)
        {
          //
        }

        public override void HighlightPotentialInteractionWithToken(bool show)
        {
          //
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
           //
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            //
        }

        public override void AnimateTo(float duration, Vector3 startPos, Vector3 endPos, Action<AbstractToken> animDone, float startScale, float endScale)
        {
            throw new NotImplementedException();
        }


        public void Closed() {
        OnWindowClosed.Invoke();
        }


        public void Show( Vector3 targetPosition, SituationEventData eventData)
		{
			if (!IsVisible)
			{
				SoundManager.PlaySfx("SituationWindowShow");
                canvasGroupFader.Show();
                DisplaySituationState(eventData);
            }

            
            positioner.Show(canvasGroupFader.durationTurnOn, targetPosition); // Animates the window (position allows optional change is position)


            results.UpdateDumpButtonText(); // ensures that we've updated the dump button accordingly
            startingSlots.ArrangeSlots(); //won't have been arranged if a card was dumped in while the window was closed
            PaginatedNotes.SetFinalPage();

        }

		public void Hide(SituationEventData eventData) {
            if (IsVisible)
            {
				SoundManager.PlaySfx("SituationWindowHide");
                DisplaySituationState(eventData);
                canvasGroupFader.Hide();
                
            }
            
        }


        public void DisplayPredictedRecipe(SituationEventData e, AspectsDictionary aspectsInSituation)
        {

            Title = e.RecipePrediction.Title;
            //Check for possible text refinements based on the aspects in context
            TextRefiner tr = new TextRefiner(aspectsInSituation);

            if(e.CurrentRecipe.HintOnly)
                PaginatedNotes.SetText("<i>" + tr.RefineString(e.RecipePrediction.DescriptiveText) + "</i>");
            else
                PaginatedNotes.SetText(tr.RefineString(e.RecipePrediction.DescriptiveText));


            if(e.CurrentRecipe.Craftable)
            {
                SoundManager.PlaySfx("SituationAvailable");
                ongoingDisplay.UpdateDisplay(e); //Ensures that the time bar is set to 0 to avoid a flicker
            }
            
        }

        
       public void SetWindowSize(bool wide) {
            RectTransform rectTrans = transform as RectTransform;

            if (wide)
                rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 900f);
            else
                rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 700f);

            if (wide != windowIsWide) {
                if (wide)
                    rectTrans.anchoredPosition = rectTrans.anchoredPosition + new Vector2(100f, 0f);
                else
                    rectTrans.anchoredPosition = rectTrans.anchoredPosition - new Vector2(100f, 0f);

                startingSlots.SetGridNumPerRow(); // Updates the grid row numbers
            }

            windowIsWide = wide;

           startingSlots.ArrangeSlots();
        }


        void DisplayButtonState(SituationEventData data) {

            switch(data.SituationState)
            {
                case SituationState.Unstarted:
            
                startButtonText.GetComponent<Babelfish>().UpdateLocLabel(NoonConstants.SITUATION_STARTABLE);
                if (data.RecipePrediction.Craftable)
                    startButton.interactable = true;
                else
                    startButton.interactable = false;
                break;

                case SituationState.Ongoing:
                
                    startButtonText.GetComponent<Babelfish>().UpdateLocLabel(NoonConstants.SITUATION_RUNNING);
                    startButton.interactable = false;
                    break;
                case SituationState.Complete:
                    startButtonText.GetComponent<Babelfish>().UpdateLocLabel(NoonConstants.SITUATION_STARTABLE);
                    startButton.interactable = false;
                    break;
            }

        }

        // ACTIONS

        public IEnumerable<ISituationNote> GetNotes() {
            return PaginatedNotes.GetCurrentTexts();
        }


        public void DisplaySituationState(SituationEventData eventData)
        {
            if (eventData.IsOpen)
            {
                startingSlots.UpdateDisplay(eventData);
                storage.UpdateDisplay(eventData);
                ongoingDisplay.UpdateDisplay(eventData);
                results.UpdateDisplay(eventData);
                results.UpdateDumpButtonText();

                DisplayButtonState(eventData);
            }

        }

        

        public void ContainerContentsUpdated(SituationEventData e)
        {
            var allAspectsInSituation = AspectsDictionary.GetFromStacks(e.StacksInEachStorage.SelectMany(s => s.Value), true);
            DisplayPredictedRecipe(e, allAspectsInSituation);


            var allAspectsToDisplay = AspectsDictionary.GetFromStacks(e.StacksInEachStorage.SelectMany(s => s.Value), false);
            aspectsDisplay.DisplayAspects(allAspectsToDisplay);


        }

        public void ReceiveNotification(SituationEventData e)
        {
            PaginatedNotes.AddText(e.Notification.Description);
        }

        public void RecipePredicted(RecipePrediction recipePrediction)
        {
            Title = recipePrediction.Title;
            PaginatedNotes.AddText(recipePrediction.DescriptiveText);
        }

        public override void MoveObject(PointerEventData eventData) {
            Vector3 dragPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(Registry.Get<IDraggableHolder>().RectTransform, eventData.position, eventData.pressEventCamera, out dragPos);

            // Potentially change this so it is using UI coords and the RectTransform?
            rectTransform.position = new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y, dragPos.z + dragHeight);

            // rotate object slightly based on pointer Delta
            if (rotateOnDrag && eventData.delta.sqrMagnitude > 10f) {
                // This needs some tweaking so that it feels more responsive, physica. Card rotates into the direction you swing it?
                perlinRotationPoint += eventData.delta.sqrMagnitude * 0.001f;
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -10 + Mathf.PerlinNoise(perlinRotationPoint, 0) * 20));
            }
        }
    }
}
