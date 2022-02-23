using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Ghosts;
using SecretHistories.Manifestations;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations
{
    public abstract class AbstractCelestialManifestation : BasicManifestation,IManifestation
    {
        [SerializeField]
        private TextMeshProUGUI _countdownText;
        public void Retire(RetirementVFX retirementVfx, Action callback)
        {
            callback();
        }

        public bool CanAnimateIcon()
        {
            return false;
        }

        public void BeginIconAnimation()
        {
            //
        }

        public void Initialise(IManifestable manifestable)
        {
            
            SetInitialTimerVisuals();

            UpdateVisuals(manifestable);

        }

        public void UpdateVisuals(IManifestable manifestable)
        {
            var timeshadow = manifestable.GetTimeshadow();

            if (timeshadow.Transient)
            {
                UpdateTimerVisuals(timeshadow.LifetimeRemaining);
            }

        }

        private void SetInitialTimerVisuals()
        {
            UpdateTimerVisuals(0f);
        }

        private void UpdateTimerVisuals(float durationRemaining)
        {
            _countdownText.text = Watchman.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage(durationRemaining);
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
            //
        }

        public void Shroud(bool instant)
        {
            //
        }

        public void Emphasise()
        {
            //
        }

        public void Understate()
        {
            //
        }

        public bool RequestingNoDrag => true;
        public bool RequestingNoSplit => true;
    
        public void DoMove(RectTransform tokenRectTransform)
        {
            //
        }

        public bool HandlePointerClick(PointerEventData eventData, Token token)
        {
            return false;
        }

        public IGhost CreateGhost()
        {
            return NullGhost.Create(this);

        }
    }
}
