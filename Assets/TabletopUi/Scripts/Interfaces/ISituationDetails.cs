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
    public interface ISituationView
    {

        string Title { get; set; }

        void Show(Vector3 targetPosOverride);
        void Hide();

        void DisplayAspects(IAspectsDictionary forAspects);
        void DisplayStartingRecipeFound(Recipe r);
    }

    public interface ISituationStorage
    {

    }


    public interface ISituationDetails {
        

        void Initialise(IVerb verb,SituationController controller);

        

        IAspectsDictionary GetAspectsFromAllSlottedElements(bool showElementAspects = true);
        IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo);

        void SetOutput(List<ElementStackToken> stacks);
        void SetUnstarted();
        void SetOngoing(Recipe forRecipe);
        void UpdateTextForPrediction(RecipePrediction recipePrediction);

        void DumpAllStartingCardsToDesktop();
        void DumpAllResultingCardsToDesktop();

        IEnumerable<ElementStackToken> GetStartingStacks();
        IEnumerable<ElementStackToken> GetOngoingStacks();
        IEnumerable<ElementStackToken> GetStoredStacks();
        IEnumerable<ElementStackToken> GetOutputStacks();

        void StoreStacks(IEnumerable<ElementStackToken> stacksToStore);
        void DisplayStoredElements();

        ElementStackTokensManager GetStorageStacksManager();
        ElementStackTokensManager GetResultsStacksManager();

        IEnumerable<ISituationNote> GetNotes();

        void SetSlotConsumptions();

		IList<RecipeSlot> GetStartingSlots();
        IList<RecipeSlot> GetOngoingSlots();
        IRecipeSlot GetUnfilledGreedySlot();
        IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo);

        void Retire();
        void SetComplete();

        void ShowDestinationsForStack(ElementStackToken stack, bool show);

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
        ElementStackToken ReprovisionExistingElementStackInStorage(ElementStackSpecification stackSpecification, Source stackSource, string locatorid = null);
        void DisplayIcon(string icon);
    }
}
