using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using TMPro;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISituationDetails {
        string Title { get; set; }

        void Initialise(IVerb verb,SituationController controller);

        void Show();
        void Hide();
        
        void DisplayAspects(IAspectsDictionary forAspects);
        void DisplayStartingRecipeFound(Recipe r);

        IAspectsDictionary GetAspectsFromAllSlottedElements(bool showElementAspects = true);
        IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo);

        void SetOutput(List<IElementStack> stacks);
        void SetUnstarted();
        void SetOngoing(Recipe forRecipe);
        void UpdateTextForPrediction(RecipePrediction recipePrediction);

        void DumpAllStartingCardsToDesktop();

        IEnumerable<IElementStack> GetStartingStacks();
        IEnumerable<IElementStack> GetOngoingStacks();
        IEnumerable<IElementStack> GetStoredStacks();
        IEnumerable<IElementStack> GetOutputStacks();

        void StoreStacks(IEnumerable<IElementStack> stacksToStore);
        void DisplayStoredElements();

        IElementStacksManager GetStorageStacksManager();

        IEnumerable<ISituationNote> GetNotes();

        void SetSlotConsumptions();

        IRecipeSlot GetUnfilledGreedySlot();
        IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo);

        void Retire();
        void SetComplete();

        void ShowDestinationsForStack(IElementStack stack);

        void DisplayTimeRemaining(float duration, float timeRemaining, Recipe recipe);
        void DisplayNoRecipeFound();
        void ReceiveNotification(INotification notification);
        IAspectsDictionary GetAspectsFromStoredElements(bool showElementAspects);
    }
}
