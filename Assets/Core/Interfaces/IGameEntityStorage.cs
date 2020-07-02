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
        void SetFutureLegacyEventRecord(string id,string value);
        void SetOrOverwritePastLegacyEventRecord(string id, string value);
        string GetFutureLegacyEventRecord(string forId);
        string GetPastLegacyEventRecord(string forId);

        Dictionary<string, string> GetAllFutureLegacyEventRecords();
        Dictionary<string, string> GetAllPastLegacyEventRecords();

        Legacy ActiveLegacy { get; set; }
    }
}
