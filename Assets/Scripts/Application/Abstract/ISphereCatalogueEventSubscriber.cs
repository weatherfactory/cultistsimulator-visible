using SecretHistories.Constants.Events;

namespace SecretHistories.Interfaces
{
    public interface ISphereCatalogueEventSubscriber
    {
        void NotifyTokensChanged(TokenInteractionEventArgs args);
        void OnTokenInteraction(TokenInteractionEventArgs args);
    }
}