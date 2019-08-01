using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;

namespace Assets.Core.Interfaces
{
    public interface IGameEntityStorage
    {
        List<IDeckInstance> DeckInstances { get; set; }
        IDeckInstance GetDeckInstanceById(string id);
        string Name { get; set; }
        string Profession { get; set; }
        void ClearExecutions();
        void AddExecutionsToHistory(string forRecipeId,int executions);
        int GetExecutionsCount(string forRecipeId);
        bool HasExhaustedRecipe(Recipe forRecipe);
        void SetFutureLegacyEventRecord(LegacyEventRecordId id,string value);
        void SetOrOverwritePastLegacyEventRecord(LegacyEventRecordId id, string value);
        string GetFutureLegacyEventRecord(LegacyEventRecordId forId);
        string GetPastLegacyEventRecord(LegacyEventRecordId forId);

        Dictionary<LegacyEventRecordId, string> GetAllFutureLegacyEventRecords();
        Dictionary<LegacyEventRecordId, string> GetAllPastLegacyEventRecords();

        Legacy ActiveLegacy { get; set; }
    }
}
