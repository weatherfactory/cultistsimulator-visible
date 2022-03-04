using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Constants;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Ghosts;
using SecretHistories.Manifestations;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.Manifestations
{
    public abstract class AbstractCelestialManifestation : BasicManifestation,IManifestation
    {
        
        [SerializeField] Image artwork;


        [SerializeField]
        private TextMeshProUGUI _countdownText;
        public override  void Retire(RetirementVFX retirementVfx, Action callback)
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

            TryOverrideVerbIcon(manifestable.GetAspects(true));


        }
        private void TryOverrideVerbIcon(AspectsDictionary forAspects)
        {
            //if we have an element in the situation now that overrides the verb icon, update it
            string overrideIcon = Watchman.Get<Compendium>().GetVerbIconOverrideFromAspects(forAspects);
            if (!string.IsNullOrEmpty(overrideIcon))
            {
                OverrideIcon(overrideIcon);
                // _window.DisplayIcon(overrideIcon);
            }
        }
        public void OverrideIcon(string art)
        {

            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(art);
            artwork.sprite = sprite;
        }
        private void SetInitialTimerVisuals()
        {
            UpdateTimerVisuals(0f);
        }

        private void UpdateTimerVisuals(float durationRemaining)
        {
            _countdownText.text = Watchman.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage(durationRemaining);
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
