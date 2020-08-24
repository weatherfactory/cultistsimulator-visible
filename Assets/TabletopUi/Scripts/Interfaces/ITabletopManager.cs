using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace TabletopUi.Scripts.Interfaces
{
    public interface ITabletopManager
    {
        void ForceAutosave();
        void ToggleLog();
        bool IsPaused();
        void SetupNewBoard(SituationBuilder builder);
        void ProvisionStartingElements(Legacy chosenLegacy, Choreographer choreographer);
        void ClearGameState(Heart h, Character s, TabletopTokenContainer tc);
        void EndGame(Ending ending, SituationController endingSituation);
        void LoadGame(int index = 0);
        IEnumerator<bool?> SaveGameAsync(bool withNotification, int index = 0, Action<bool> callback = null);
        HashSet<TokenAndSlot> FillTheseSlotsWithFreeStacks(HashSet<TokenAndSlot> slotsToFill);
        void CloseAllDetailsWindows();
        void CloseAllSituationWindowsExcept(string exceptTokenId);
        bool IsSituationWindowOpen();
        void SetHighlightedElement(string elementId, int quantity = 1);
        void DecayStacksOnTable(float interval);
        void DecayStacksInResults(float interval);
        void SetPausedState(bool paused);
        bool GetPausedState();
        void SetAutosaveInterval(float minutes);
        void SetGridSnapSize(float snapsize);
        void ShowMansusMap(SituationController situation, Transform origin, PortalEffect effect);
        void ReturnFromMansus(Transform origin, ElementStackToken mansusCard);
        void BeginNewSituation(SituationCreationCommand scc, List<IElementStack> withStacksInStorage);
        void SignalImpendingDoom(ISituationAnchor situationToken);
        void NoMoreImpendingDoom(ISituationAnchor situationToken);
        void HighlightAllStacksForSlotSpecificationOnTabletop(SlotSpecification slotSpec);
        AspectsInContext GetAspectsInContext(IAspectsDictionary aspectsInSituation);
        void GroupAllStacks();
        void PurgeElement(string elementId, int maxToPurge);
        void HaltVerb(string verbId, int maxToHalt);
        void DeleteVerb(string verbId, int maxToDelete);

    }
}
