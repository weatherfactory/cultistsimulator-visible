
namespace SecretHistories.Fucine
{
    public interface ICharacterSubscriber
    {
        public void CharacterNameUpdated(string newName);
        public void CharacterProfessionUpdated(string newProfession);
    }
}
