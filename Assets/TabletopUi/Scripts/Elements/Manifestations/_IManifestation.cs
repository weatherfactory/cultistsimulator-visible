using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
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
        void ResetIconAnimation();


        void InitialiseVisuals(Element element);
        void InitialiseVisuals(IVerb verb);

        void UpdateVisuals(Element element, int quantity);
        void UpdateTimerVisuals(float originalDuration, float durationRemaining, float interval, bool resaturate, EndingFlavour signalEndingFlavour);
        void OverrideIcon(string icon);

        void OnBeginDragVisuals();
        void OnEndDragVisuals();

        void TravelTo(Token token, float duration, Vector3 startPos, Vector3 endPos,
            Action travelComplete, float startScale = 1f, float endScale = 1f);


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


        bool HandleClick(PointerEventData eventData, Token token);

        void DisplaySpheres(IEnumerable<Sphere> spheres);
        
        
        

        /// <summary>
        /// needs to be set to initial token container
        /// </summary>
        /// <param name="transform"></param>
        void SetParticleSimulationSpace(Transform transform);



    }
}
