
using System.Collections.Generic;
using SecretHistories.Assets.Scripts.Application.Infrastructure;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;


namespace SecretHistories.Infrastructure.Persistence

{
    /// <summary>
    /// ccompletely new game, ab initio
    /// </summary>
    public class FreshGameProvider: GamePersistenceProvider
    {
        public Legacy StartingLegacy { get; set; }
        private Dictionary<string, string> _historyRecordsFromPreviousCharacter;
        private readonly AbstractTokenSetupChamberlain _chamberlain;


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
            var chamberlain = Watchman.Get<AbstractTokenSetupChamberlain>();
            VersionNumber currentVersion = Watchman.Get<MetaInfo>().VersionNumber;
            _persistedGameState= PersistedGameState.ForLegacy(StartingLegacy, _historyRecordsFromPreviousCharacter, currentVersion);

        }

        

    }
}
