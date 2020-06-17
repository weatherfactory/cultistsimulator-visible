using Assets.Core.Entities;

namespace Assets.Editor
{
    public class SimulatedRollOverride : IRollOverride
    {
        private readonly ISituationSimulatorSubscriber _subscriber;

        public SimulatedRollOverride(ISituationSimulatorSubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public int PopNextOverrideValue(Recipe recipe = null)
        {
            return _subscriber.OnRecipeRollRequested(recipe);
        }
    }
}
