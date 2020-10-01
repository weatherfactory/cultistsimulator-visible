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
        void SetUnstarted();
        void SetOngoing(Recipe situationCurrentPrimaryRecipe);
        void SetComplete();
        IEnumerable<ElementStackToken> GetOngoingStacks();
        IAspectsDictionary GetAspectsFromAllSlottedElements(bool includeElementAspects);
        IAspectsDictionary GetAspectsFromAllSlottedAndStoredElements(bool includeElementAspects);
        IAspectsDictionary GetAspectsFromOutputElements(bool includeElementAspects);
        IEnumerable<ElementStackToken> GetOutputStacks();
        IEnumerable<ElementStackToken> GetStartingStacks();
        void Retire();
        ElementStacksManager GetStorageStacksManager();
        void SetSlotConsumptions();
        IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo);
        IEnumerable<ISituationNote> GetNotes();
        void TryDecayResults(float interval);
        void DumpAllResultingCardsToDesktop();
        void ReceiveTextNote(INotification notification);
        IEnumerable<ElementStackToken> GetStoredStacks();
        ElementStacksManager GetResultsStacksManager();
        void DumpAllStartingCardsToDesktop();
        IAspectsDictionary GetAspectsFromStoredElements(bool includeElementAspects);
        void SetOutput(List<ElementStackToken> stacksForOutput);
        IRecipeSlot GetUnfilledGreedySlot();
        void StoreStacks(IEnumerable<ElementStackToken> getStartingStacks);
        IList<RecipeSlot> GetOngoingSlots();
    }


    //public interface ISituationDetails {
        

    //    void Initialise(IVerb verb,SituationController controller);

        

    //    IAspectsDictionary GetAspectsFromAllSlottedElements(bool showElementAspects = true);
    //    IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo);

    //    void SetOutput(List<ElementStackToken> stacks);
    //    void SetUnstarted();
    //    void SetOngoing(Recipe forRecipe);
    //    IAspectsDictionary GetAspectsFromAllSlottedElements(bool showElementAspects = true);
    //    IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo);

    //    IEnumerable<ElementStackToken> GetStartingStacks();
    //    IEnumerable<ElementStackToken> GetOngoingStacks();
    //    IEnumerable<ElementStackToken> GetStoredStacks();
    //    IEnumerable<ElementStackToken> GetOutputStacks();
    //    void StoreStacks(IEnumerable<ElementStackToken> stacksToStore);
    //    ElementStacksManager GetStorageStacksManager();
    //    ElementStacksManager GetResultsStacksManager();
    //    IEnumerable<ISituationNote> GetNotes();
    //    IList<RecipeSlot> GetStartingSlots();
    //    IList<RecipeSlot> GetOngoingSlots();
    //    IRecipeSlot GetUnfilledGreedySlot();
    //    IAspectsDictionary GetAspectsFromAllSlottedAndStoredElements(bool showElementAspects);
    //    void TryDecayResults(float interval);

    //    void ReceiveTextNote(INotification notification);
    //    IAspectsDictionary GetAspectsFromStoredElements(bool showElementAspects);
    //    IAspectsDictionary GetAspectsFromOutputElements(bool showElementAspects);
    //    void SetSlotConsumptions();

    //    void DumpAllStartingCardsToDesktop();
    //    void DumpAllResultingCardsToDesktop();
    //    void Retire();
    //    void SetComplete();
    //}


}
