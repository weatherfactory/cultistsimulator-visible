using SecretHistories.Constants.Events;

namespace SecretHistories.Interfaces
{
    public interface ISphereCatalogueEventSubscriber
    {
        void NotifyTokensChanged(SphereContentsChangedEventArgs args);
        void OnTokenInteraction(TokenInteractionEventArgs args);
    }
}