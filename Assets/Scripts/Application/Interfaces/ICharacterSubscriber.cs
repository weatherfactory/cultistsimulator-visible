
namespace SecretHistories.Fucine
{
    public interface ICharacterSubscriber
    {
         void CharacterNameUpdated(string newName);
         void CharacterProfessionUpdated(string newProfession);
    }
}
