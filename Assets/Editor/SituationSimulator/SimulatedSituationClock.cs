using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Interfaces;

namespace Assets.Editor
{
    public class SimulatedSituationClock : ISituationClock
    {
        public SituationState State { get; set; }
        public float TimeRemaining
        {
            get { return 0; }
        }

        public float Warmup
        {
            get { return 0; }
        }

        public string RecipeId
        {
            get { return _currentPrimaryRecipe == null ? null : _currentPrimaryRecipe.Id; }
        }

        private Recipe _currentPrimaryRecipe;
        private readonly SituationController _controller;
        private readonly ISituationSimulatorSubscriber _simulationSubscriber;

        public SimulatedSituationClock(
            Recipe primaryRecipe, SituationController controller, ISituationSimulatorSubscriber simulationSubscriber)
        {
            _currentPrimaryRecipe = primaryRecipe;
            _simulationSubscriber = simulationSubscriber;
            _controller = controller;
            State = SituationState.Unstarted;
        }

        public IList<SlotSpecification> GetSlotsForCurrentRecipe()
        {
            return _currentPrimaryRecipe.Slots.Any() ? _currentPrimaryRecipe.Slots :
                new List<SlotSpecification>();
        }

        public string GetTitle()
        {
            return _currentPrimaryRecipe == null ? "no recipe just now" :
                _currentPrimaryRecipe.Label;
        }

        public string GetStartingDescription()
        {
            return _currentPrimaryRecipe == null ? "no recipe just now" :
                _currentPrimaryRecipe.StartDescription;
        }

        public string GetDescription()
        {
            return _currentPrimaryRecipe == null ? "no recipe just now" :
                _currentPrimaryRecipe.Description;
        }

        public SituationState Continue(IRecipeConductor rc, float interval, bool waitForGreedyAnim = false)
        {
            // Process all states that should appear in the normal flow
            switch (State)
            {
                case SituationState.FreshlyStarted:
                    Beginning(_currentPrimaryRecipe);
                    break;
                case SituationState.Ongoing:
                    RequireExecution(rc);
                    break;
                case SituationState.RequiringExecution:
                    End(rc);
                    break;
                case SituationState.Complete:
                    break;
                case SituationState.Unstarted:
                    throw new SituationSimulatorException("Failed to find a valid recipe to start");
            }

            return State;
        }

        public RecipePrediction GetPrediction(IRecipeConductor rc)
        {
            return rc.GetRecipePrediction(_currentPrimaryRecipe);
        }

        public void Beginning(Recipe withRecipe)
        {
            State = SituationState.Ongoing;
            _controller.SituationBeginning(withRecipe);
            _simulationSubscriber.OnRecipeStarted(withRecipe, GetOngoingSlotsManager());
        }

        public void Start(Recipe primaryRecipe)
        {
            State = SituationState.FreshlyStarted;
            _currentPrimaryRecipe = primaryRecipe;
        }

        public void ResetIfComplete()
        {
            if (State == SituationState.Complete)
                Reset();
        }

        public void Halt()
        {
            Complete();
        }

        private void Reset() {
            _currentPrimaryRecipe = null;
            State = SituationState.Unstarted;
            _controller.ResetSituation();
        }

        private void RequireExecution(IRecipeConductor rc) {
            State = SituationState.RequiringExecution;

            IList<RecipeExecutionCommand> recipeExecutionCommands = rc.GetActualRecipesToExecute(_currentPrimaryRecipe);

            if (recipeExecutionCommands.First().Recipe.Id != _currentPrimaryRecipe.Id)
            {
                _simulationSubscriber.OnRecipeExecuted(_currentPrimaryRecipe);
                _currentPrimaryRecipe = recipeExecutionCommands.First().Recipe;
            }

            foreach (var c in recipeExecutionCommands) {
                ISituationEffectCommand ec = new SituationEffectCommand(
                    c.Recipe, c.Recipe.ActionId != _currentPrimaryRecipe.ActionId, c.Expulsion);
                _controller.SituationExecutingRecipe(ec);
            }
        }

        private void End(IRecipeConductor rc) {
            var linkedRecipe = rc.GetLinkedRecipe(_currentPrimaryRecipe);
            _simulationSubscriber.OnRecipeExecuted(_currentPrimaryRecipe);
            if (linkedRecipe != null)
            {
                _currentPrimaryRecipe = linkedRecipe;
                Beginning(_currentPrimaryRecipe);
            }
            else
            {
                Complete();
            }
        }

        private void Complete() {
            State = SituationState.Complete;
            _controller.SituationComplete();
            _simulationSubscriber.OnSituationCompleted(_controller.situationWindow.GetOutputStacks(), GetDescription());
        }

        private SimulatedOngoingSlotsManager GetOngoingSlotsManager()
        {
            SimulatedSituationDetails details = _controller.situationWindow as SimulatedSituationDetails;
            if (details == null)
                throw new SituationSimulatorException("Invalid situation details");
            return details.GetOngoingSlotManager();
        }
    }
}
