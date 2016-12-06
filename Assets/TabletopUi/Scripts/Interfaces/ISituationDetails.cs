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
        void Initialise(SituationController controller);
        void Show(bool situationOngoing);
        void Hide();
        void DisplayAspects(IAspectsDictionary forAspects);
        void DisplayRecipe(Recipe r);
        IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo);
        IEnumerable<IElementStack> GetStacksInStartingSlots();
        AspectsDictionary GetAspectsFromSlottedElements();
        void AddOutput(IEnumerable<IElementStack> stacks,INotification notification);
        void DisplayStarting();
        void DisplayOngoing();
        void DisplaySituation(string stitle, string sdescription, string nextRecipeDescription);
        void Retire();
    }
}
