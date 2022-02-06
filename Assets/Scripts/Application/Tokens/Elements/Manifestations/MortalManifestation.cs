using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Tokens.Ghosts;
using SecretHistories.Enums;
using SecretHistories.Ghosts;
using SecretHistories;
using SecretHistories.Services;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations
{
 [RequireComponent(typeof(RectTransform))]
    public class MortalManifestation: BasicManifestation, IManifestation
    {
        [SerializeField]
        private GameObject _emphasisGlow;

        public void Retire(RetirementVFX retirementVfx, Action callback)
        {
            throw new NotImplementedException();
        }

        public bool CanAnimateIcon()
        {
            throw new NotImplementedException();
        }

        public void BeginIconAnimation()
        {
            throw new NotImplementedException();
        }

        public void Initialise(IManifestable manifestable)
        {
           //
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
            //
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

        public bool NoPush { get; }
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
          _emphasisGlow.SetActive(true);
        }

        public void Understate()
        {
          _emphasisGlow.SetActive(false);

        }

        public bool RequestingNoDrag => true;
        public bool RequestingNoSplit { get; }

        //eg Someones which can only be directed, not dragged

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
            var newGhost = Watchman.Get<PrefabFactory>()
                .CreateGhostPrefab(typeof(MortalGhost), this.RectTransform);
            return newGhost;
        }
    }
}
