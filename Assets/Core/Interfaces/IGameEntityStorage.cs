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
        string PreviousCharacterName { get; set; }
        string ReplaceTextFor(string text);
        void AddExecutionsToHistory(string forRecipeId,int executions);
        int GetExecutionsCount(string forRecipeId);
        bool HasExhaustedRecipe(Recipe forRecipe);
        void SetLegacyEventRecord(LegacyEventRecordId id,string value);
       string GetLegacyEventRecord(LegacyEventRecordId forId);

    }
}
