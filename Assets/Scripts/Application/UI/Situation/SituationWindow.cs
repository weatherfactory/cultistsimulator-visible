﻿#pragma warning disable 0649
using System;
using System.Collections.Generic;
using SecretHistories.Interfaces;
using SecretHistories.UI.SlotsContainers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;

using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SecretHistories.UI {

    [Serializable]
    public class OnContainerAddedEvent : UnityEvent<Sphere> { }

    [Serializable]
    public class OnContainerRemovedEvent : UnityEvent<Sphere> { }


    [RequireComponent(typeof(SituationWindowPositioner))]
    public class SituationWindow : MonoBehaviour, ISituationSubscriber {

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
        [SerializeField] Output results;
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
        private SituationPath _situationPath;
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

        public void TryResizeWindow(int slotsCount)
        {
            SetWindowSize(slotsCount > 3);
        }
        public void DisplayIcon(string icon)
        {
            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(icon);
            artwork.sprite = sprite;
        }
         
        public void  Initialise(Situation situation) {
			Verb = situation.Verb;
            _situationPath = situation.Path;
            name = "Window_" + Verb.Id;
            DisplayIcon(Verb.Id);

            Title = Verb.Label;
            PaginatedNotes.SetText(Verb.Description);
            startButton.onClick.AddListener(OnStart.Invoke);

            startingSlots.Initialise(Verb, this,_situationPath);
           storage.Initialise(situation);


            ongoingDisplay.Initialise(situation);
            results.Initialise(situation);

            //this is an improvement - the situation doesn't need to know what to add - but better yet would be to tie together creation + container add, at runtime
            foreach (var s in startingSlots.GetAllSlots())
                OnContainerAdded.Invoke(s);

            if (Verb.Startable)
            {
                startButton.gameObject.SetActive(true);
            }
            else
            {
                startButton.gameObject.SetActive(false);
            }

            SituationStateChanged(situation);
            TimerValuesChanged(situation);


        }


        public void Closed() {
          OnWindowClosed.Invoke();
        }


        public void Show( Vector3 targetPosition, Situation situation)
		{
			if (!IsVisible)
			{
				SoundManager.PlaySfx("SituationWindowShow");
                canvasGroupFader.Show();
                SituationStateChanged(situation);
            }

            positioner.Show(canvasGroupFader.durationTurnOn, targetPosition); // Animates the window (position allows optional change is position)

            startingSlots.ArrangeSlots(); //won't have been arranged if a card was dumped in while the window was closed
            PaginatedNotes.SetFinalPage();

        }

		public void Hide(Situation s) {
            if (IsVisible)
            {
				SoundManager.PlaySfx("SituationWindowHide");
                SituationStateChanged(s);
                canvasGroupFader.Hide();
                
            }
            
        }

        public void Collect()
        {
            OnCollect.Invoke();
        }


        public void DisplayPredictedRecipe(Situation s)
        {

            Title = s.CurrentRecipePrediction.Title;
            
            if(s.CurrentRecipePrediction. HintOnly)
                PaginatedNotes.SetText("<i>" + s.CurrentRecipePrediction.DescriptiveText + "</i>");
            else
                PaginatedNotes.SetText(s.CurrentRecipePrediction.DescriptiveText);


            if(s.CurrentPrimaryRecipe.Craftable)
            {
                SoundManager.PlaySfx("SituationAvailable");
              //  ongoingDisplay.UpdateDisplay(s); //Ensures that the time bar is set to 0 to avoid a flicker - commented this out while refactoring, but bear it in mind
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


        void DisplayButtonState(Situation situation) {

            if (situation.TimeRemaining > 0)
            {
                startButtonText.GetComponent<Babelfish>().UpdateLocLabel(NoonConstants.SITUATION_RUNNING);
                startButton.interactable = false;
            }
            else if (situation.CurrentRecipePrediction!=null && situation.CurrentRecipePrediction.Craftable)
            {
                startButtonText.GetComponent<Babelfish>().UpdateLocLabel(NoonConstants.SITUATION_STARTABLE);
                startButton.interactable = true;
            }
            else
            {
                startButtonText.GetComponent<Babelfish>().UpdateLocLabel(NoonConstants.SITUATION_STARTABLE);
                startButton.interactable = false;
            }

        }

        // ACTIONS

        public IEnumerable<ISituationNote> GetNotes() {
            return PaginatedNotes.GetCurrentTexts();
        }

        public void SituationStateChanged(Situation situation)
        {
      
                startingSlots.UpdateDisplay(situation);

                DisplayButtonState(situation);
        }

        public void TimerValuesChanged(Situation situation)
        {
  //
        }


        public void SituationSphereContentsUpdated(Situation s)
        {
            DisplayPredictedRecipe(s);
            
            var allAspectsToDisplay =s.GetAspectsAvailableToSituation(false);
            aspectsDisplay.DisplayAspects(allAspectsToDisplay);
            DisplayButtonState(s);

        }

        public void ReceiveNotification(INotification n)
        {
            PaginatedNotes.AddText(n.Description);
        }
         

        //public override void MoveObject(PointerEventData eventData) {
        //    Vector3 dragPos;
        //    RectTransformUtility.ScreenPointToWorldPointInRectangle(Registry.Get<IDraggableHolder>().RectTransform, eventData.position, eventData.pressEventCamera, out dragPos);

        //    // Potentially change this so it is using UI coords and the RectTransform?
        //    rectTransform.position = new Vector3(dragPos.x + dragOffset.x, dragPos.y + dragOffset.y, dragPos.z + dragHeight);

        //    // rotate object slightly based on pointer Delta
        //    if (rotateOnDrag && eventData.delta.sqrMagnitude > 10f) {
        //        // This needs some tweaking so that it feels more responsive, physica. Card rotates into the direction you swing it?
        //        perlinRotationPoint += eventData.delta.sqrMagnitude * 0.001f;
        //        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -10 + Mathf.PerlinNoise(perlinRotationPoint, 0) * 20));
        //    }
        //}
        public void Retire()
        {
            Destroy(gameObject);
        }
    }
}