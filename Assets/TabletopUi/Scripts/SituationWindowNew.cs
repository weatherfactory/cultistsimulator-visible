using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.SlotsContainers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
namespace Assets.CS.TabletopUI {
    public class SituationWindowNew : MonoBehaviour, ISituationDetails {

        public string Description {
            get {
                throw new NotImplementedException();
            }

            set {
                throw new NotImplementedException();
            }
        }

        public string Title {
            get {
                throw new NotImplementedException();
            }

            set {
                throw new NotImplementedException();
            }
        }

        // 

        
        // ACTIONS

        public void ShowPrevNote() {

        }

        public void ShowNextNote() {
            
        }

        void StartRecipe() {
            
        }

        // ISituationDetails

        public void ConsumeMarkedElements() {
            throw new NotImplementedException();
        }

        public void DisplayAspects(IAspectsDictionary forAspects) {
            throw new NotImplementedException();
        }

        public void DisplayNoRecipeFound() {
            throw new NotImplementedException();
        }

        public void DisplayStartingRecipeFound(Recipe r) {
            throw new NotImplementedException();
        }

        public AspectsDictionary GetAspectsFromAllSlottedElements() {
            throw new NotImplementedException();
        }

        public IAspectsDictionary GetAspectsFromStoredElements() {
            throw new NotImplementedException();
        }

        public IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo) {
            throw new NotImplementedException();
        }

        public IEnumerable<ISituationOutputNote> GetOutputNotes() {
            throw new NotImplementedException();
        }

        public IEnumerable<IElementStack> GetOutputStacks() {
            throw new NotImplementedException();
        }

        public ElementStacksManager GetSituationStorageStacksManager() {
            throw new NotImplementedException();
        }

        public IEnumerable<IElementStack> GetStacksInOngoingSlots() {
            throw new NotImplementedException();
        }

        public IEnumerable<IElementStack> GetStacksInStartingSlots() {
            throw new NotImplementedException();
        }

        public IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo) {
            throw new NotImplementedException();
        }

        public IEnumerable<IElementStack> GetStoredStacks() {
            throw new NotImplementedException();
        }

        public IRecipeSlot GetUnfilledGreedySlot() {
            throw new NotImplementedException();
        }

        public void Hide() {
            throw new NotImplementedException();
        }

        public void Initialise(IVerb verb, SituationController controller) {
            throw new NotImplementedException();
        }

        public void Retire() {
            throw new NotImplementedException();
        }

        public void SetComplete() {
            throw new NotImplementedException();
        }

        public void SetOngoing(Recipe forRecipe) {
            throw new NotImplementedException();
        }

        public void SetOutput(List<IElementStack> stacks, INotification notification) {
            throw new NotImplementedException();
        }

        public void SetSlotConsumptions() {
            throw new NotImplementedException();
        }

        public void SetUnstarted() {
            throw new NotImplementedException();
        }

        public void Show() {
            throw new NotImplementedException();
        }

        public void ShowDestinationsForStack(IElementStack stack) {
            throw new NotImplementedException();
        }

        public void StoreStacks(IEnumerable<IElementStack> stacksToStore) {
            throw new NotImplementedException();
        }

        public void UpdateTextForPrediction(RecipePrediction recipePrediction) {
            throw new NotImplementedException();
        }

    }
}
