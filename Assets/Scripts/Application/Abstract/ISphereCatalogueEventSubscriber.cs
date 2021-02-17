using SecretHistories.Constants.Events;

namespace SecretHistories.Fucine
{
    public interface ISphereCatalogueEventSubscriber
    {
        void NotifyTokensChanged(SphereContentsChangedEventArgs args);
        void OnTokenInteraction(TokenInteractionEventArgs args);
    }
}