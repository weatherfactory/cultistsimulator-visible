#pragma warning disable 0649
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Assets.Scripts.Application.Infrastructure.Events;
using Assets.Scripts.Application.UI.Situation;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Events;
using SecretHistories.Spheres;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace SecretHistories.UI
{




    [RequireComponent(typeof(WindowPositioner))]
    public class MortalWindow : MonoBehaviour, ISituationSubscriber, IPayloadWindow
    {


        //[SerializeField] private StateEnum CurrentStateIdentifier;
        //  [SerializeField] private string CurrentRecipeId;

        [Header("Visuals")] [SerializeField] CanvasGroupFader canvasGroupFader;
        public WindowPositioner positioner;

        [Space] [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private float MinHeightBeforeExpansion;



        [Space] [SerializeField] List<SituationDominion> Dominions;


        [Space] [SerializeField] AspectsDisplay aspectsDisplay;

        [SerializeField] Button startButton;
        [SerializeField] TextMeshProUGUI startButtonText;

        public UnityEvent OnStart;
        public UnityEvent OnCollect;
        public UnityEvent OnWindowClosed;
        public OnSphereAddedEvent OnSphereAdded;
        public OnSphereRemovedEvent OnSphereRemoved;


        //  public TokenLocation LastOpenLocation;

        private ITokenPayload _payload;


        public bool IsVisible
        {
            get { return canvasGroupFader.IsFullyVisible() || canvasGroupFader.IsAppearing(); }
        }

        public string Title
        {
            get { return title.text; }
            set { title.text = value; }
        }

        public Vector3 Position
        {
            get { return positioner.GetPosition(); }
            set { positioner.SetPosition(value); }
        }

        public void Attach(ElementStack newElementStack)
        {
            throw new NotImplementedException("Can't attach a SituationWindow to an element stack");
        }

        public void Attach(Situation newSituation)
        {

            newSituation.AddSubscriber(this);

            OnWindowClosed.AddListener(newSituation.Close);
            OnStart.AddListener(newSituation.TryStart);
            OnCollect.AddListener(newSituation.Conclude);
            OnSphereAdded.AddListener(newSituation.AttachSphere);
            OnSphereRemoved.AddListener(newSituation.DetachSphere);


            startButton.onClick.AddListener(OnStart.Invoke);

            foreach (var d in Dominions)
                d.RegisterFor(newSituation);

            startButton.gameObject.SetActive(true);


            SituationStateChanged(newSituation);
            TimerValuesChanged(newSituation);

            _payload = newSituation;
            _payload.OnChanged += OnPayloadChanged;

            name = "Window_" + _payload.EntityId;
            DisplayIcon(_payload.Icon);

            Title = _payload.Label;


        }

        private void OnPayloadChanged(TokenPayloadChangedArgs args)
        {
            Title = _payload.Label;

            //This slightly obscurely named method is now where we open and close the window

            if (args.ChangeType == PayloadChangeType.Retirement)
                Retire();
            if (args.ChangeType == PayloadChangeType.Opening && !this.IsVisible)
                PayloadRequestsShow(args.Payload.GetRectTransform().position);

            if (args.ChangeType == PayloadChangeType.Closing && this.IsVisible)
                PayloadRequestsHide();



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


        public void PayloadRequestsShow(Vector3 startPosition)
        {
            if (!IsVisible)
            {
                SoundManager.PlaySfx("SituationWindowShow");
                canvasGroupFader.Show();
                positioner.Show(canvasGroupFader.durationTurnOn, startPosition); // Animates the window (position allows optional change in position)
                NotifySpheresChanged(new Context(Context.ActionSource.SphereReferenceLocationChanged));
            }


        }

        public void PayloadRequestsHide()
        {
            if (IsVisible)
            {
                SoundManager.PlaySfx("SituationWindowHide");
                canvasGroupFader.Hide();
                NotifySpheresChanged(new Context(Context.ActionSource.SphereReferenceLocationChanged));
            }

        }

        public void Collect()
        {
            OnCollect.Invoke();
        }





        public void ContentsDisplayChanged(ContentsDisplayChangedArgs args)
        {


            layoutElement.minHeight = MinHeightBeforeExpansion + args.ExtraHeightRequested;
        }


        void DisplayButtonState(Situation situation)
        {

            if (situation.TimeRemaining > 0)
            {
                startButtonText.GetComponent<Babelfish>().UpdateLocLabel(NoonConstants.SITUATION_RUNNING);
                startButton.interactable = false;
            }
            else if (situation.CurrentRecipePrediction != null && situation.CurrentRecipePrediction.Craftable && situation.StateIdentifier == StateEnum.Unstarted)
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
            //   CurrentStateIdentifier = situation.StateIdentifier;
            //    CurrentRecipeId = situation.RecipeId;
        }

        public void TimerValuesChanged(Situation situation)
        {
            //
        }


        public void SituationSphereContentsUpdated(Situation s)
        {
            //If we have a transient card changing lifetime - and perhaps in other circs - this can get called every heartbeat. Candidate for optimisation.   
            var allAspectsToDisplay = s.GetAspects(false);
            aspectsDisplay.DisplayAspects(allAspectsToDisplay);
            DisplayButtonState(s);

        }



        public void Retire()
        {
            _payload.OnChanged -= OnPayloadChanged;
            Destroy(gameObject);
        }



        /// <summary>
        /// Something's happened that affects our spheres. Anything with an interest in its constituent spheres (like tokens travelling to 'em) will want to know.  It might be that this should go into the Situation.
        /// </summary>
        public void NotifySpheresChanged(Context context)
        {
            List<Sphere> spheres = new List<Sphere>();
            foreach (var d in Dominions)
            {
                spheres.AddRange(d.Spheres);
            }

            foreach (var sphere in spheres)
            {
                SphereChangedArgs args;


                args = new SphereChangedArgs(sphere, context);
                sphere.NotifySphereChanged(args);
            }

        }
    }
}
