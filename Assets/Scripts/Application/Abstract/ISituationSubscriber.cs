
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Commands;
using SecretHistories.Entities;

namespace SecretHistories.Fucine
{
    public interface ISituationSubscriber
    {
        void SituationStateChanged(Situation situation);
        void TimerValuesChanged(Situation s);
        void SituationSphereContentsUpdated(Situation s);


    }
}
