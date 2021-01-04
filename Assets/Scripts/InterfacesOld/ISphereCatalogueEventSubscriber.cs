using SecretHistories.Infrastructure.Events;

namespace SecretHistories.Interfaces
{
    public interface ISphereCatalogueEventSubscriber
    {
        void NotifyTokensChanged(TokenInteractionEventArgs args);
        void OnTokenInteraction(TokenInteractionEventArgs args);
    }
}