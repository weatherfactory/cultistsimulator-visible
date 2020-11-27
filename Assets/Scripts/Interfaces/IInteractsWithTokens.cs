using Assets.CS.TabletopUI;

namespace Assets.Scripts.UI
{
    public interface IInteractsWithTokens
    {
        bool CanInteractWithToken(Token token);
        void ShowPossibleInteractionWithToken(Token token);
        void StopShowingPossibleInteractionWithToken(Token token);
    }
}