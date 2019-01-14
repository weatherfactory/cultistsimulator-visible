using System.Collections.Generic;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Interfaces;

namespace Assets.Editor
{
    public class SimulatedSituationAnchor : ISituationAnchor
    {
        public string EntityId
        {
            get { return _verb == null ? null : _verb.Id; }
        }
        public SituationController SituationController { get; private set; }
        public string SaveLocationInfo { get; set; }

        public bool IsTransient
        {
            get { return _verb.Transient; }
        }
        public bool EditorIsActive
        {
            get { return false;  }
        }
        private IVerb _verb;

        public SimulatedSituationAnchor(IVerb verb, SituationController controller)
        {
            Initialise(verb, controller, null);
        }

        public void DisplayAsOpen()
        {
        }

        public void DisplayAsClosed()
        {
        }

        public void Initialise(IVerb verb, SituationController controller, Heart heart)
        {
            _verb = verb;
            SituationController = controller;
        }

        public void DisplayMiniSlot(IList<SlotSpecification> ongoingSlots)
        {
        }

        public void DisplayTimeRemaining(float duration, float timeRemaining, EndingFlavour signalEndingFlavour)
        {
        }

        public void DisplayStackInMiniSlot(IEnumerable<IElementStack> getStacksInOngoingSlots)
        {
        }

        public void DisplayComplete()
        {
        }

        public bool Retire()
        {
            return true;
        }

        public void SetCompletionCount(int newCount)
        {
        }

        public void SetGlowColor(UIStyle.TokenGlowColor colorType)
        {
        }

        public void ShowGlow(bool glowState, bool instant = false)
        {
        }

        public void SetEditorActive(bool active)
        {
        }

        public SlotSpecification GetPrimarySlotSpecificationForVerb()
        {
            return _verb.PrimarySlotSpecification;
        }

        public void DisplayIcon(IVerb v)
        {
            //hakuna matata
        }

        public void DisplayIcon(string icon)
        {
           //hakuna matata
        }
    }
}
