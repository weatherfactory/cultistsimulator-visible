
namespace SecretHistories.Interfaces
{
    public interface ICharacterSubscriber
    {
         void CharacterNameUpdated(string newName);
         void CharacterProfessionUpdated(string newProfession);
    }
}
