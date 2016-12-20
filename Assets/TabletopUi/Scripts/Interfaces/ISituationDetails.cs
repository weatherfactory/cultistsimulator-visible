using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISituationDetails
    {
        void Initialise(IVerb verb,SituationController controller);
        void Show();
        void Hide();
        void DisplayAspects(IAspectsDictionary forAspects);
        void DisplayRecipe(Recipe r);
        IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo);
        IEnumerable<IElementStack> GetStacksInStartingSlots();
        AspectsDictionary GetAspectsFromSlottedElements();
        void AddOutput(IEnumerable<IElementStack> stacks,INotification notification);
        void DisplayStarting();
        void DisplayOngoing(Recipe forRecipe);
        void UpdateSituationDisplay(string stitle, string sdescription, string nextRecipeDescription);
        void Retire();
        IEnumerable<ISituationOutput> GetCurrentOutputs();
        void RunSlotConsumptions();
        IEnumerable<IElementStack> GetStacksInOngoingSlots();
        IList<IRecipeSlot> GetUnfilledGreedySlots();
        IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo);
    }
}
