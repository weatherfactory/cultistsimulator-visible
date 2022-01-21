using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Enums;
using SecretHistories.Manifestations;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Assets.Scripts.Application.Tokens.Elements.Manifestations
{
 [RequireComponent(typeof(RectTransform))]
    public class MortalManifestation: BasicManifestation, IManifestation
    {

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
            throw new NotImplementedException();
        }

        public void UpdateVisuals(IManifestable manifestable)
        {
            throw new NotImplementedException();
        }

        public void OnBeginDragVisuals()
        {
            throw new NotImplementedException();
        }

        public void OnEndDragVisuals()
        {
            throw new NotImplementedException();
        }

        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
            throw new NotImplementedException();
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
            throw new NotImplementedException();
        }

        public bool NoPush { get; }
        public void Unshroud(bool instant)
        {
            throw new NotImplementedException();
        }

        public void Shroud(bool instant)
        {
            throw new NotImplementedException();
        }

        public void Emphasise()
        {
            throw new NotImplementedException();
        }

        public void Understate()
        {
            throw new NotImplementedException();
        }

        public bool RequestingNoDrag { get; }
        public bool RequestingNoSplit { get; }
        public void DoMove(RectTransform tokenRectTransform)
        {
            throw new NotImplementedException();
        }

        public bool HandlePointerClick(PointerEventData eventData, Token token)
        {
            throw new NotImplementedException();
        }

        public IGhost CreateGhost()
        {
            throw new NotImplementedException();
        }
    }
}
