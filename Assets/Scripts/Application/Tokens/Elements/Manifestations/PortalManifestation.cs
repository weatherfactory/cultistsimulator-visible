using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Ghosts;
using SecretHistories.Spheres;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.Manifestations
{
    [RequireComponent(typeof(RectTransform))]
    public class PortalManifestation : MonoBehaviour, IManifestation
    {
        public Transform Transform => gameObject.transform;
        public RectTransform RectTransform => gameObject.GetComponent<RectTransform>();

        private List<Sprite> frames;
        [SerializeField] Image artwork;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI egressMessage;
        [SerializeField] private MinimalDominion soleDominion;
        public OutputSphere EgressOutput;
        public CanvasGroupFader Fader;



        public void Retire(RetirementVFX retirementVfx, Action callback)
        {
            
            Fader.SetOnChangeCompleteCallback(callback);
            Fader.Hide();
        }

        public bool CanAnimateIcon()
        {
            return false;
        }

        public void BeginIconAnimation()
        {
         //
        }

        public void ResetIconAnimation()
        {
      //
        }

        public void UpdateLocalScale(Vector3 newScale)
        {
            RectTransform.localScale = newScale;
        }

        public void OnBeginDragVisuals()
        {
         //
        }

        public void OnEndDragVisuals()
        {
//
        }

        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
      //
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
            //
        }

        public bool NoPush => true;
        public void Unshroud(bool instant)
        {
            
        }

        public void Shroud(bool instant)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void Emphasise()
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void Understate()
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void DoRevealEffect(bool instant)
        {
        //
        }

        public void DoShroudEffect(bool instant)
        {
        //
        }

        public bool RequestingNoDrag => true;
        public bool RequestingNoSplit => true;

        public void DoMove(RectTransform tokenRectTransform)
        {
            //
        }

        public void Initialise(IManifestable manifestable)
        {

            Fader.SetStatesForFinalAlpha(0f); //make it invisible; alpha isn't set to 0 cos it's a nuisance when editing the prefab
            Fader.Show();
    UpdateVisuals(manifestable);

    soleDominion.RegisterFor(manifestable);
            
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
            string art = manifestable.Icon;

            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(art);
            frames = ResourcesManager.GetAnimFramesForVerb(art);
            artwork.sprite = sprite;
            title.text = manifestable.Label;
            egressMessage.text = manifestable.Description;

        }


        public void UpdateTimerVisuals(float originalDuration, float durationRemaining, float interval, bool resaturate,
            EndingFlavour signalEndingFlavour)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void SendNotification(INotification notification)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public void UpdateTimerVisuals(float duration, float timeRemaining, EndingFlavour signalEndingFlavour)
        {
            //
        }


        public void DisplayStackInMiniSlot(ElementStack stack)
        {
            //
        }

        public void DisplayComplete()
        {
            //
        }

        public void SetCompletionCount(int count)
        {
            //
        }

        public void ReceiveAndRefineTextNotification(INotification notification)
        {
            //
        }

        public bool HandlePointerDown(PointerEventData eventData, Token token)
        {
            return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
            NoonUtility.LogWarning(this.GetType().Name + " doesn't support this operation");
        }

        public IGhost CreateGhost()
        {
            return NullGhost.Create(this);
        }



    }
}
