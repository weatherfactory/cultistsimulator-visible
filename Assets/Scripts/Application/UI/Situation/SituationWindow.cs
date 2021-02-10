#pragma warning disable 0649
using System;
using System.Collections.Generic;
using System.Numerics;
using Assets.Scripts.Application.Infrastructure.Events;
using Assets.Scripts.Application.UI.Situation;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace SecretHistories.UI {

    [Serializable]
    public class OnSphereAddedEvent : UnityEvent<Sphere> { }

    [Serializable]
    public class OnSphereRemovedEvent : UnityEvent<Sphere> { }


    [RequireComponent(typeof(SituationWindowPositioner))]
    public class SituationWindow : MonoBehaviour, ISituationSubscriber {

        [Header("Visuals")]
		[SerializeField] CanvasGroupFader canvasGroupFader;
        public SituationWindowPositioner positioner;

        [Space]
        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI title;



        [Space]
        [SerializeField] List<Dominion> Dominions;


        [Space]
        [SerializeField] AspectsDisplay aspectsDisplay;

		[SerializeField] Button startButton;
		[SerializeField] TextMeshProUGUI startButtonText;

        public UnityEvent OnStart;
        public UnityEvent OnCollect;
        public UnityEvent OnWindowClosed;
        public OnSphereAddedEvent OnSphereAdded;
        public OnSphereRemovedEvent OnSphereRemoved;

        public TokenLocation LastOpenLocation;

        private ITokenPayload _payload; //Ideally, we would reduce this to an ITokenPayload

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

        public void Attach(Situation newSituation)
        {
        

            newSituation.AddSubscriber(this);

            OnWindowClosed.AddListener(newSituation.Close);
            OnStart.AddListener(newSituation.TryStart);
            OnCollect.AddListener(newSituation.Conclude);
            OnSphereAdded.AddListener(newSituation.AttachSphere);
            OnSphereRemoved.AddListener(newSituation.RemoveSphere);

            
            startButton.onClick.AddListener(OnStart.Invoke);


            foreach (var d in Dominions)
                d.RegisterFor(newSituation);
            
            startButton.gameObject.SetActive(true);
 

            SituationStateChanged(newSituation);
            TimerValuesChanged(newSituation);

            _payload = newSituation;
            _payload.OnChanged += OnPayloadChanged;

            name = "Window_" + _payload.Id;
            DisplayIcon(_payload.Id);

            Title = _payload.Label;

            //I took this out for the sake of a test; but it's a shitshow anyway
        //    positioner.SetInitialPosition(initialLocation.Anchored3DPosition);
     
        }

        private void OnPayloadChanged(TokenPayloadChangedArgs args)
        {
            if(args.ChangeType==PayloadChangeType.Retirement)
                Retire();
            if(_payload.IsOpen && !this.IsVisible)
                PayloadRequestsShow(Vector3.zero);

            if(!_payload.IsOpen && this.IsVisible)
                PayloadRequestsHide();
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

        public void CloseButtonClicked()
        {
            OnWindowClosed.Invoke(); //calls down to payload, which should then call up again
        }


        public void PayloadRequestsShow( Vector3 startPosition)
		{
			if (!IsVisible)
			{
				SoundManager.PlaySfx("SituationWindowShow");
                canvasGroupFader.Show();
                positioner.Show(canvasGroupFader.durationTurnOn, startPosition); // Animates the window (position allows optional change in position)
            }


        }

		public void PayloadRequestsHide() {
            if (IsVisible)
            {
				SoundManager.PlaySfx("SituationWindowHide");
                canvasGroupFader.Hide();
            }
            
        }

        public void Collect()
        {
            OnCollect.Invoke();
        }


        public void DisplayPredictedRecipe(Situation s)
        {


            if(s.Recipe.Craftable)
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
            }

            windowIsWide = wide;

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

        public void SituationStateChanged(Situation situation)
        {
            
                DisplayButtonState(situation);
        }

        public void TimerValuesChanged(Situation situation)
        {
  //
        }


        public void SituationSphereContentsUpdated(Situation s)
        {
            DisplayPredictedRecipe(s);
            
            var allAspectsToDisplay =s.GetAspects(false);
            aspectsDisplay.DisplayAspects(allAspectsToDisplay);
            DisplayButtonState(s);

        }


        public void ReceiveCommand(IAffectsTokenCommand command)
        {
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
            _payload.OnChanged -= OnPayloadChanged;
            Destroy(gameObject);
        }

 
    }
}
