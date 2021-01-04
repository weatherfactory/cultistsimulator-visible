using SecretHistories.UI;

namespace SecretHistories.UI
{
    public interface IInteractsWithTokens
    {
        bool CanInteractWithToken(Token token);
        void ShowPossibleInteractionWithToken(Token token);
        void StopShowingPossibleInteractionWithToken(Token token);
    }
}