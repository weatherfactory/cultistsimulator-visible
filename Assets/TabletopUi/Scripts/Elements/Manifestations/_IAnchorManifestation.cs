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
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.TabletopUi.Scripts.Elements.Manifestations
{
    public interface IAnchorManifestation: IManifestation
    {
        void InitialiseVisuals(IVerb verb);
        void UpdateTimerVisuals(float duration, float timeRemaining, EndingFlavour signalEndingFlavour);

        
        void ShowMiniSlot(bool greedy);
        void HideMiniSlot();
        void DisplayStackInMiniSlot(ElementStackToken stack);
        void DisplayComplete();
        void SetCompletionCount(int count);
        void ReceiveAndRefineTextNotification(INotification notification);
        bool HandleClick(PointerEventData eventData,VerbAnchor anchor);
        void OverrideIcon(string icon);
        Vector3 GetOngoingSlotPosition();



        /// <summary>
        /// needs to be set to initial token container
        /// </summary>
        /// <param name="transform"></param>
        void SetParticleSimulationSpace(Transform transform);

        void AnimateTo(IArtAnimatableToken token, float duration, Vector3 startPos, Vector3 endPos,
            Action<VerbAnchor> SituationAnimDone, float startScale = 1f, float endScale = 1f);
    }

}
