using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Situation;
using UnityEngine;

namespace Assets.CS.TabletopUI
{
   public class BookshelfAnchor: AbstractSituationAnchor, ISituationAnchor
    {
        public void StartArtAnimation()
        {
            throw new NotImplementedException();
        }

        public IEnumerator DoAnim(float duration, int frameCount, int frameIndex)
        {
            throw new NotImplementedException();
        }

        public bool CanAnimate()
        {
            throw new NotImplementedException();
        }

        public string EntityId { get; }
        public SituationController SituationController { get; }
        public string SaveLocationInfo { get; set; }
        public bool IsTransient { get; }
        public void DisplayAsOpen()
        {
            throw new NotImplementedException();
        }

        public void DisplayAsClosed()
        {
            throw new NotImplementedException();
        }

        public void Initialise(IVerb verb, SituationController controller)
        {
            throw new NotImplementedException();
        }

        public void DisplayMiniSlot(IList<SlotSpecification> ongoingSlots)
        {
            throw new NotImplementedException();
        }

        public void DisplayTimeRemaining(float duration, float timeRemaining, EndingFlavour signalEndingFlavour)
        {
            throw new NotImplementedException();
        }

        public void DisplayStackInMiniSlot(IEnumerable<ElementStackToken> getStacksInOngoingSlots)
        {
            throw new NotImplementedException();
        }

        public void DisplayComplete()
        {
            throw new NotImplementedException();
        }

        public bool Retire()
        {
            throw new NotImplementedException();
        }

        public void SetCompletionCount(int newCount)
        {
            throw new NotImplementedException();
        }

        public void SetGlowColor(UIStyle.TokenGlowColor colorType)
        {
            throw new NotImplementedException();
        }

        public void ShowGlow(bool glowState, bool instant = false)
        {
            throw new NotImplementedException();
        }

        public void DisplayOverrideIcon(string icon)
        {
            throw new NotImplementedException();
        }

        public void SetParticleSimulationSpace(Transform transform)
        {
            throw new NotImplementedException();
        }

        public void SetTokenContainer(ITokenContainer newContainer, Context context)
        {
            throw new NotImplementedException();
        }

        public RectTransform RectTransform { get; }
        public void DisplayAtTableLevel()
        {
            throw new NotImplementedException();
        }

        public void SnapToGrid()
        {
            throw new NotImplementedException();
        }

        public bool NoPush { get; }
    }
}
