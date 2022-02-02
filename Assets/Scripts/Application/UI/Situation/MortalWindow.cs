using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Enums;
using SecretHistories.Events;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SecretHistories.UI
{
    [RequireComponent(typeof(WindowPositioner))]
    public class MortalWindow: MonoBehaviour,ISituationSubscriber,IPayloadWindow
    {
        private ITokenPayload _payload;
        public UnityEvent OnStart;
        
        public UnityEvent OnWindowClosed;
        public OnSphereAddedEvent OnSphereAdded;
        public OnSphereRemovedEvent OnSphereRemoved;

        [Header("Visuals")] [SerializeField] CanvasGroupFader canvasGroupFader;
        public WindowPositioner positioner;

        [Space] [SerializeField] Image _artwork;
        [SerializeField] TextMeshProUGUI _title;

        [Space] [SerializeField] List<SituationDominion> Dominions;

        //TODO: make sure windowPositioner can call back here OK

        public void SituationStateChanged(SecretHistories.Entities.Situation situation)
        {
            //
        }

        public void TimerValuesChanged(SecretHistories.Entities.Situation s)
        {
            //
        }

        public void SituationSphereContentsUpdated(SecretHistories.Entities.Situation s)
        {
            //
        }

        public string Title
        {
            get => _title.text;
            set => _title.text = value;
        }

        public Vector3 Position
        {
            get => positioner.GetPosition();
            set => positioner.SetPosition(value);
        }

        public bool IsVisible
        {
            get { return canvasGroupFader.IsFullyVisible() || canvasGroupFader.IsAppearing(); }
        }

        public void Attach(ElementStack elementStack)
        {
            throw new NotImplementedException("Can't attach a MortalWindow to an element stack");
        }

        public void Attach(SecretHistories.Entities.Situation newSituation)
        {

            newSituation.AddSubscriber(this);

            OnWindowClosed.AddListener(newSituation.Close);
            OnStart.AddListener(newSituation.TryStart);
            

            OnSphereAdded.AddListener(newSituation.AttachSphere);
            OnSphereRemoved.AddListener(newSituation.DetachSphere);

foreach (var d in Dominions)
                d.RegisterFor(newSituation);



            SituationStateChanged(newSituation);
            TimerValuesChanged(newSituation);

            _payload = newSituation;
            _payload.OnChanged += OnPayloadChanged;

            name = "MortalWindow_" + _payload.EntityId;
            DisplayIcon(_payload.Icon);

            Title = _payload.Label;
        }
        public void DisplayIcon(string icon)
        {
            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(icon);
            _artwork.sprite = sprite;
        }
        private void OnPayloadChanged(TokenPayloadChangedArgs args)
        {
            _title.text = _payload.Label;

            //This slightly obscurely named method is now where we open and close the window

            if (args.ChangeType == PayloadChangeType.Retirement)
                Retire();
            if (args.ChangeType == PayloadChangeType.Opening && !this.IsVisible)
                PayloadRequestsShow(args.Payload.GetRectTransform().position);

            if (args.ChangeType == PayloadChangeType.Closing && this.IsVisible)
                PayloadRequestsHide();
            
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
        public void CloseButtonClicked()
        {
            OnWindowClosed.Invoke(); //calls down to payload, which should then call up again
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
        public void Retire()
        {
            _payload.OnChanged -= OnPayloadChanged;
            Destroy(gameObject);
        }
    }
}
