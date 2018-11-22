using System.Collections.Generic;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Editor
{
    public class SimulatedTabletopManager : ITabletopManager
    {
        public ISituationSimulatorSubscriber _simulationSubscriber;

        public SimulatedTabletopManager(ISituationSimulatorSubscriber simulationSubscriber)
        {
            _simulationSubscriber = simulationSubscriber;
        }
        public void ForceAutosave()
        {
        }

        public void ToggleLog()
        {
        }

        public bool IsPaused()
        {
            return true;
        }

        public void SetupNewBoard(SituationBuilder builder)
        {
        }

        public void ProvisionStartingElements(Legacy chosenLegacy, Choreographer choreographer)
        {
        }

        public void ClearGameState(Heart h, IGameEntityStorage s, TabletopTokenContainer tc)
        {
        }

        public void RestartGame()
        {
        }

        public void EndGame(Ending ending, SituationController endingSituation)
        {
        }

        public void LoadGame(int index = 0)
        {
        }

        public bool SaveGame(bool withNotification, int index = 0)
        {
            return true;
        }

        public HashSet<TokenAndSlot> FillTheseSlotsWithFreeStacks(HashSet<TokenAndSlot> slotsToFill)
        {
            return null;
        }

        public void CloseAllSituationWindowsExcept(string exceptTokenId)
        {
        }

        public void DecayStacksOnTable(float interval)
        {
        }

        public void DecayStacksInResults(float interval)
        {
        }

        public void SetPausedState(bool paused)
        {
        }

        public bool GetPausedState()
        {
            return true;
        }

        public void SetAutosaveInterval(float minutes)
        {
        }

        public void SetGridSnapSize(float snapsize)
        {
        }

        public void ShowMansusMap(SituationController situation, Transform origin, PortalEffect effect)
        {
        }

        public void ReturnFromMansus(Transform origin, ElementStackToken mansusCard)
        {
        }

        public void BeginNewSituation(SituationCreationCommand scc, List<IElementStack> withStacksInStorage)
        {
            _simulationSubscriber.OnRecipeExpulsion(scc.Verb, scc.Recipe, withStacksInStorage);

            // Remove the stacks from their previous stack manager, since they won't get reassigned to a new situation
            foreach (var stack in withStacksInStorage)
                stack.SetStackManager(null);
        }

        public void SignalImpendingDoom(ISituationAnchor situationToken)
        {
        }

        public void NoMoreImpendingDoom(ISituationAnchor situationToken)
        {
        }

        public void HighlightAllStacksForSlotSpecificationOnTabletop(SlotSpecification slotSpec)
        {
        }
    }
}
