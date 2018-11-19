using System.Collections.Generic;
using Assets.Core.Interfaces;

namespace Assets.Editor
{
    public interface ISituationSimulatorSubscriber
    {
        void OnRecipeStarted(Recipe recipe, SimulatedSlotsManager ongoingSlotsManager);
        int OnRecipeRollRequested(Recipe recipe);
        void OnRecipeExecuted(Recipe recipe);
        void OnRecipeExpulsion(IVerb verb, Recipe recipe, List<IElementStack> stacks);
        void OnSituationCompleted(IEnumerable<IElementStack> outputStacks, string outputText);
    }
}
