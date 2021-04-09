using SecretHistories.Constants.Events;
using SecretHistories.Events;

namespace SecretHistories.Fucine
{
    public interface ISphereCatalogueEventSubscriber
    {
        void OnSphereChanged(SphereChangedArgs args);
        void OnTokensChanged(SphereContentsChangedEventArgs args);
        void OnTokenInteraction(TokenInteractionEventArgs args);
    }
}