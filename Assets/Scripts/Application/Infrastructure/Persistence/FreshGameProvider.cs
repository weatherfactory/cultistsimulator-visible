
using SecretHistories.Entities;
using SecretHistories.Enums;



namespace SecretHistories.Infrastructure.Persistence

{
    /// <summary>
    /// ccompletely new game, ab initio
    /// </summary>
    public class FreshGameProvider: GamePersistenceProvider
    {
        public Legacy StartingLegacy { get; set; }


        protected override string GetSaveFileLocation()
        {
            return string.Empty;
        }


        public override GameSpeed GetDefaultGameSpeed()
        {
            return GameSpeed.Normal;
        }

        public FreshGameProvider(Legacy startingLegacy)
        {
            StartingLegacy = startingLegacy;
        }

        public override void DepersistGameState()
        {
            _persistedGameState= PersistedGameState.ForLegacy(StartingLegacy);

        }

        

    }
}
