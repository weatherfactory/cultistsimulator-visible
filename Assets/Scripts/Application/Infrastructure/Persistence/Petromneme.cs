
namespace SecretHistories.Infrastructure.Persistence
{
   public class Petromneme: PersistedGame
    {
        public override string GetSaveFileLocation()
        {
            return $"{UnityEngine.Application.persistentDataPath}/save.txt";
        }

   
    }
}
