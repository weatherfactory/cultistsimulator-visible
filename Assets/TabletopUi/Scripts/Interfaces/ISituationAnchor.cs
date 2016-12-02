using System;
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
        string LocationInfo { get; }
        bool IsTransient { get; }
        string CurrentRecipeId { get; }
        void OpenToken();
        void CloseToken();

        void Initialise(IVerb verb, SituationController controller);

        IEnumerable<IElementStack> GetStoredStacks();
        void StoreStacks(IEnumerable<IElementStack> stacksToStore);
        IAspectsDictionary GetAspectsFromStoredElements();
        IAspectsDictionary GetAspectsFromSlottedElements();
        void SituationBeginning(IList<SlotSpecification> ongoingSlots);
        void SituationExtinct();

        IList<IRecipeSlot> GetUnfilledGreedySlots();
        void DisplayTimeRemaining(float duration, float timeRemaining);
        void AbsorbOngoingSlotContents();
        void ModifyStoredElementStack(string elementId, int quantity);

        HeartbeatResponse ExecuteHeartbeat(float interval);

        bool Retire();
    }
}
