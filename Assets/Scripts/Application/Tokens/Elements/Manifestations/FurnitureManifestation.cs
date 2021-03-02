using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations
{
    public class FurnitureManifestation: MonoBehaviour, IManifestation
    {
        public Transform Transform { get; }
        public RectTransform RectTransform { get; }
        public void Retire(RetirementVFX retirementVfx, Action callback)
        {
            throw new NotImplementedException();
        }

        public bool CanAnimateIcon()
        {
            return false;
        }

        public void BeginIconAnimation()
        {
          //
        }

        public void InitialiseVisuals(IManifestable manifestable)
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

        public void Highlight(HighlightType highlightType)
        {
            throw new NotImplementedException();
        }

        public void Unhighlight(HighlightType highlightType)
        {
            throw new NotImplementedException();
        }

        public bool NoPush { get; }
        public void Reveal(bool instant)
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
        public void DoMove(RectTransform tokenRectTransform)
        {
            throw new NotImplementedException();
        }

        public void SendNotification(INotification notification)
        {
            throw new NotImplementedException();
        }

        public bool HandlePointerDown(PointerEventData eventData, Token token)
        {
            throw new NotImplementedException();
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
            throw new NotImplementedException();
        }

        public void SetParticleSimulationSpace(Transform transform)
        {
            throw new NotImplementedException();
        }
    }
}
