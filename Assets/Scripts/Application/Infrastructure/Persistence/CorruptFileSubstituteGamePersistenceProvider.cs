using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Infrastructure;
using SecretHistories.Entities;
using SecretHistories.UI;

namespace SecretHistories.Infrastructure.Persistence
{
    internal class CorruptFileSubstituteGamePersistenceProvider: GamePersistenceProvider
    {
        protected override string GetSaveFileLocation()
        {
            string persistentDataPath = Watchman.Get<MetaInfo>().PersistentDataPath;

            return $"{persistentDataPath}/save.json";
        }

        public override void DepersistGameState()
        {
            
           
        }
        public override bool IsValid()
        {
            return false;
        }
    }
}
