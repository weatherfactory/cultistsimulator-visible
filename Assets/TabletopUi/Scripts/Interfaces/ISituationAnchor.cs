using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISituationAnchor
    {
        string Id { get; }

        SituationController SituationController { get; }

        string SaveLocationInfo { get; set; }
        bool IsTransient { get; }
        bool EditorIsActive { get; }

        void DisplayAsOpen();
        void DisplayAsClosed();

        void Initialise(IVerb verb, SituationController controller, Heart heart);

        void DisplayMiniSlot(IList<SlotSpecification> ongoingSlots);
        void DisplayTimeRemaining(float duration, float timeRemaining, Recipe recipe);
        void DisplayStackInMiniSlot(IEnumerable<IElementStack> getStacksInOngoingSlots);
        void DisplayComplete();
        bool Retire();

        void SetCompletionCount(int newCount);

        void SetGlowColor(UIStyle.TokenGlowColor colorType);
        void ShowGlow(bool glowState, bool instant = false);
        void SetEditorActive(bool active);
        SlotSpecification GetPrimarySlotSpecificationForVerb();
    }
}
