using Assets.TabletopUi.Scripts.Infrastructure.Events;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISphereCatalogueEventSubscriber
    {
        void NotifyTokensChanged(TokenInteractionEventArgs args);
        void OnTokenInteraction(TokenInteractionEventArgs args);
    }
}