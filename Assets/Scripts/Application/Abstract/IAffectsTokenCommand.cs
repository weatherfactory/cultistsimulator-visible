using SecretHistories.Abstract;
using SecretHistories.UI;

namespace SecretHistories.Commands
{
    public interface IAffectsTokenCommand
    {
        bool ExecuteOn(Token token);
        bool ExecuteOn(ITokenPayload payload);

    }
}