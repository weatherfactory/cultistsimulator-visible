using System.Collections.Generic;
using Assets.Core.Interfaces;

namespace Assets.Editor
{
    public interface ISituationSimulatorSubscriber
    {
        void OnRecipeStarted(Recipe recipe, SimulatedSlotsManager ongoingSlotsManager);
        int OnRecipeRollRequested(Recipe recipe);
        void OnRecipeExecuted(Recipe recipe);
        void OnSituationCompleted(IEnumerable<IElementStack> outputStacks, string outputText);
    }
}
