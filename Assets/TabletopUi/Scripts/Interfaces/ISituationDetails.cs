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
        void UpdateTextForPrediction(RecipePrediction recipePrediction);
        void DisplayStoredElements();
        void DisplayTimeRemaining(float duration, float timeRemaining, EndingFlavour endingFlavour);
        void DisplayNoRecipeFound();
        void DisplayIcon(string icon);
        void DisplayHintRecipeFound(Recipe r);
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
        IAspectsDictionary GetAspectsFromAllSlottedElements(bool showElementAspects = true);
        IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo);

        IEnumerable<IElementStack> GetStartingStacks();
        IEnumerable<IElementStack> GetOngoingStacks();
        IEnumerable<IElementStack> GetStoredStacks();
        IEnumerable<IElementStack> GetOutputStacks();
        void StoreStacks(IEnumerable<IElementStack> stacksToStore);
        IElementStacksManager GetStorageStacksManager();
        IElementStacksManager GetResultsStacksManager();
        IEnumerable<ISituationNote> GetNotes();
        IList<RecipeSlot> GetStartingSlots();
        IList<RecipeSlot> GetOngoingSlots();
        IRecipeSlot GetUnfilledGreedySlot();
        IAspectsDictionary GetAspectsFromAllSlottedAndStoredElements(bool showElementAspects);
        void TryDecayResults(float interval);

        void ReceiveTextNote(INotification notification);
        IAspectsDictionary GetAspectsFromStoredElements(bool showElementAspects);
        IAspectsDictionary GetAspectsFromOutputElements(bool showElementAspects);
        void SetSlotConsumptions();

        void DumpAllStartingCardsToDesktop();
        void DumpAllResultingCardsToDesktop();
        void Retire();
        void SetComplete();
    }


}
