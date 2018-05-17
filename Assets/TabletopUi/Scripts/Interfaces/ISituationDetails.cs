using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using TMPro;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISituationDetails {
        string Title { get; set; }

        void Initialise(IVerb verb,SituationController controller, Heart heart);

        void Show( Vector3 targetPosOverride );
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
        void DumpAllResultingCardsToDesktop();

        IEnumerable<IElementStack> GetStartingStacks();
        IEnumerable<IElementStack> GetOngoingStacks();
        IEnumerable<IElementStack> GetStoredStacks();
        IEnumerable<IElementStack> GetOutputStacks();

        void StoreStacks(IEnumerable<IElementStack> stacksToStore);
        void DisplayStoredElements();

        IElementStacksManager GetStorageStacksManager();
        IElementStacksManager GetResultsStacksManager();

        IEnumerable<ISituationNote> GetNotes();

        void SetSlotConsumptions();

        IList<RecipeSlot> GetOngoingSlots();
        IRecipeSlot GetUnfilledGreedySlot();
        IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo);

        void Retire();
        void SetComplete();

        void ShowDestinationsForStack(IElementStack stack, bool show);

        void DisplayTimeRemaining(float duration, float timeRemaining, EndingFlavour endingFlavour);
        void DisplayNoRecipeFound();
        void ReceiveTextNote(INotification notification);
        IAspectsDictionary GetAspectsFromStoredElements(bool showElementAspects);
        IAspectsDictionary GetAspectsFromOutputElements(bool showElementAspects);
        void DisplayHintRecipeFound(Recipe r);
        IAspectsDictionary GetAspectsFromAllSlottedAndStoredElements(bool showElementAspects);
        void TryDecayResults(float interval);

		// Added to allow saving of window positions. Better than inserting save code into the SituationDetails IMHO - CP
		Vector3 Position { get; set; }
        void SetWindowSize(bool wide);
    }
}
