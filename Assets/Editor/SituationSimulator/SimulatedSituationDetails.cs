using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Editor
{
    public class SimulatedSituationDetails : ISituationDetails
    {
        public string Title { get; set; }

        public Vector3 Position { get; set; }

        private SituationController _situationController;

        private SimulatedStartingSlotsManager _startingSlots;

        private SimulatedOngoingSlotsManager _ongoingSlots;

        private SimulatedTokenContainer _storage;

        private SimulatedTokenContainer _results;

        public void Initialise(IVerb verb, SituationController controller, Heart heart)
        {
            _situationController = controller;
            _startingSlots = new SimulatedStartingSlotsManager(controller);
            _ongoingSlots = new SimulatedOngoingSlotsManager(controller);
            _storage = new SimulatedTokenContainer("storage");
            _results = new SimulatedTokenContainer("results");
        }

        public void Show(Vector3 targetPosOverride)
        {
        }

        public void Hide()
        {
        }

        public void DisplayAspects(IAspectsDictionary forAspects)
        {
        }

        public void DisplayStartingRecipeFound(Recipe r)
        {
            Title = r.Label;
        }

        public IAspectsDictionary GetAspectsFromAllSlottedElements(bool showElementAspects = true)
        {
            var slottedAspects = new AspectsDictionary();
            slottedAspects.CombineAspects(_startingSlots.GetAspectsFromSlottedCards(showElementAspects));
            slottedAspects.CombineAspects(_ongoingSlots.GetAspectsFromSlottedCards(showElementAspects));
            return slottedAspects;
        }

        public IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo)
        {
            return null;
        }

        public void SetOutput(List<IElementStack> stacks)
        {
            if (stacks.Any() == false)
                return;
            _results.GetElementStacksManager().AcceptStacks(stacks, new Context(Context.ActionSource.SituationResults));
        }

        public void SetUnstarted()
        {
            _startingSlots.DoReset();
            _ongoingSlots.DoReset();
        }

        public void SetOngoing(Recipe forRecipe)
        {
            _ongoingSlots.SetupSlot(forRecipe);
        }

        public void UpdateTextForPrediction(RecipePrediction recipePrediction)
        {
            Title = recipePrediction.Title;
        }

        public void DumpAllStartingCardsToDesktop()
        {
        }

        public void DumpAllResultingCardsToDesktop()
        {
            _situationController.ResetSituation();
        }

        public IEnumerable<IElementStack> GetStartingStacks()
        {
            return _startingSlots.GetStacksInSlots();
        }

        public IEnumerable<IElementStack> GetOngoingStacks()
        {
            return _ongoingSlots.GetStacksInSlots();
        }

        public IEnumerable<IElementStack> GetStoredStacks()
        {
            return GetStorageStacksManager().GetStacks();
        }

        public IEnumerable<IElementStack> GetOutputStacks()
        {
            return _results.GetElementStacksManager().GetStacks();
        }

        public bool TryAddStackToStartingSlot(string slotId, IElementStack stack)
        {
            return _startingSlots.TryAddStackToSlot(slotId, stack);
        }

        public void StoreStacks(IEnumerable<IElementStack> stacksToStore)
        {
            GetStorageStacksManager().AcceptStacks(stacksToStore, new Context(Context.ActionSource.SituationStoreStacks));
            _startingSlots.RemoveAnyChildSlotsWithEmptyParent();
        }

        public void DisplayStoredElements()
        {
        }

        public IElementStacksManager GetStorageStacksManager()
        {
            return _storage.GetElementStacksManager();
        }

        public IElementStacksManager GetResultsStacksManager()
        {
            return _results.GetElementStacksManager();
        }

        public IEnumerable<ISituationNote> GetNotes()
        {
            return null;
        }

        public void SetSlotConsumptions()
        {
            foreach (var s in _startingSlots.GetAllSlots())
                s.SetConsumption();

            foreach (var o in _ongoingSlots.GetAllSlots())
                o.SetConsumption();
        }

        public IList<RecipeSlot> GetStartingSlots()
        {
            return null;
        }

        public IList<RecipeSlot> GetOngoingSlots()
        {
            return null;
        }

        public SimulatedOngoingSlotsManager GetOngoingSlotManager()
        {
            return _ongoingSlots;
        }

        public IRecipeSlot GetUnfilledGreedySlot()
        {
            return null;
        }

        public IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo)
        {
            return _ongoingSlots.GetSlotBySaveLocationInfoPath(locationInfo);
        }

        public void Retire()
        {
        }

        public void SetComplete()
        {
        }

        public void ShowDestinationsForStack(IElementStack stack, bool show)
        {
        }

        public void DisplayTimeRemaining(float duration, float timeRemaining, EndingFlavour endingFlavour)
        {
        }

        public void DisplayNoRecipeFound()
        {
        }

        public void ReceiveTextNote(INotification notification)
        {
        }

        public IAspectsDictionary GetAspectsFromStoredElements(bool showElementAspects)
        {
            return GetStorageStacksManager().GetTotalAspects(showElementAspects);
        }

        public IAspectsDictionary GetAspectsFromOutputElements(bool showElementAspects)
        {
            return GetResultsStacksManager().GetTotalAspects(showElementAspects);
        }

        public void DisplayHintRecipeFound(Recipe r)
        {
        }

        public IAspectsDictionary GetAspectsFromAllSlottedAndStoredElements(bool showElementAspects)
        {
            var aspects = GetAspectsFromAllSlottedElements(showElementAspects);
            var storedAspects = GetAspectsFromStoredElements(showElementAspects);
            aspects.CombineAspects(storedAspects);
            return aspects;
        }

        public void TryDecayResults(float interval)
        {
            var stacksToDecay = GetResultsStacksManager().GetStacks();
            foreach (var s in stacksToDecay)
                s.Decay(interval);
        }

        public void SetWindowSize(bool wide)
        {
        }

        public IElementStack ReprovisionExistingElementStackInStorage(
            ElementStackSpecification stackSpecification, Source stackSource, string locatorId = null)
        {
            return _storage.ReprovisionExistingElementStack(stackSpecification, stackSource, locatorId);
        }
    }
}
