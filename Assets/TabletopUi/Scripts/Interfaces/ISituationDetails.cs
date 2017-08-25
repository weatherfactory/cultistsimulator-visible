using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
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
        void DisplayStartingRecipeFound(Recipe r);
        IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo);
        IEnumerable<IElementStack> GetStacksInStartingSlots();
        AspectsDictionary GetAspectsFromAllSlottedElements();
        void SetOutput(IEnumerable<IElementStack> stacks,INotification notification);
        void SetStarting();
        void SetOngoing(Recipe forRecipe);
        void UpdateTextForPrediction(RecipePrediction recipePrediction);
        
        IEnumerable<IElementStack> GetOutputCards();
        void SetSlotConsumptions();
        IEnumerable<IElementStack> GetStacksInOngoingSlots();
        IRecipeSlot GetUnfilledGreedySlot();
        IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo);

        IEnumerable<IElementStack> GetStoredStacks();
        void StoreStacks(IEnumerable<IElementStack> stacksToStore);
        IAspectsDictionary GetAspectsFromStoredElements();
        ElementStacksManager GetSituationStorageStacksManager();


        void Retire();
        void SetComplete();
        void ConsumeMarkedElements();
        void ShowDestinationsForStack(IElementStack stack);
        void DisplayNoRecipeFound();
    }
}
