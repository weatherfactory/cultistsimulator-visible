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
        void OpenToken();
        void CloseToken();

        HeartbeatResponse ExecuteHeartbeat(float interval);

        void Initialise(IVerb verb, SituationController controller);

        void DisplayMiniSlotDisplay(IList<SlotSpecification> ongoingSlots);
        void DisplayTimeRemaining(float duration, float timeRemaining, Recipe recipe);
        void UpdateMiniSlotDisplay(IEnumerable<IElementStack> getStacksInOngoingSlots);
        void DisplayComplete();
        bool Retire();

        void SetCompletionCount(int newCount);
        int GetCompletionCount();
    }
}
