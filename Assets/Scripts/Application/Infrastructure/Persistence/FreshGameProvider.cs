
using System.Collections.Generic;
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
        private Dictionary<string, string> _historyRecordsFromPreviousCharacter;


        protected override string GetSaveFileLocation()
        {
            return string.Empty;
        }


        public override GameSpeed GetDefaultGameSpeed()
        {
            return GameSpeed.Normal;
        }


        public FreshGameProvider(Legacy startingLegacy,Dictionary<string,string> withHistory)
        {
            _historyRecordsFromPreviousCharacter = withHistory;
            StartingLegacy = startingLegacy;
        }

        public FreshGameProvider(Legacy startingLegacy)
        {
            _historyRecordsFromPreviousCharacter=new Dictionary<string, string>();
            StartingLegacy = startingLegacy;
        }

        public override void DepersistGameState()
        {
            _persistedGameState= PersistedGameState.ForLegacy(StartingLegacy,_historyRecordsFromPreviousCharacter);

        }

        

    }
}
