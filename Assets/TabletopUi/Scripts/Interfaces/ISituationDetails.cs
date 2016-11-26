using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISituationDetails
    {
        void Initialise(SituationController controller);
        void Show();
        void Hide();
        void DisplayOngoing();
        void DisplayStarting();
        void DisplayAspects(IAspectsDictionary forAspects);
        void DisplayRecipe(Recipe r);
        IEnumerable<IElementStack> GetStacksInStartingSlots();
        AspectsDictionary GetAspectsFromSlottedElements();
        void AddOutput(IEnumerable<IElementStack> stacks,INotification notification);
        void DisplaySituation(string stitle, string sdescription, string nextRecipeDescription);
        void Retire();
    }
}
