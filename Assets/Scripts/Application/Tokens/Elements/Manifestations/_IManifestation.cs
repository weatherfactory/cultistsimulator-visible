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
using SecretHistories.Spheres;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations
{
    public enum HighlightType
    {
        CanFitSlot,
        CanMerge,
        All,
        Hover,
        AttentionPls,
        CanInteractWithOtherToken
    }

    public interface IManifestation
    {
        Transform Transform { get; }
       RectTransform RectTransform { get; }
        void Retire(RetirementVFX retirementVfx, Action callback);
        bool CanAnimateIcon();
        void BeginIconAnimation();
        

        void InitialiseVisuals(IManifestable manifestable);

        void UpdateVisuals(IManifestable manifestable);

        void OnBeginDragVisuals();
        void OnEndDragVisuals();

        void Highlight(HighlightType highlightType);
        void Unhighlight(HighlightType highlightType);
        bool NoPush { get; }
        void Reveal(bool instant);
        void Shroud(bool instant);
        void Emphasise();
        void Understate();
        bool RequestingNoDrag { get; }
        void DoMove(RectTransform tokenRectTransform);

        void SendNotification(INotification notification);


        bool HandlePointerDown(PointerEventData eventData, Token token);

        void DisplaySpheres(IEnumerable<Sphere> spheres);
        
        
        /// <summary>
        /// needs to be set to initial token container
        /// </summary>
        /// <param name="transform"></param>
        void SetParticleSimulationSpace(Transform transform);



    }
}
