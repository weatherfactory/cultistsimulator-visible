using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using UnityEngine;

namespace SecretHistories.Infrastructure.Persistence
{
    public class DevSlotSavePersistableGameState: PersistableGameState
    {
        private readonly int _slotNumber;

        public DevSlotSavePersistableGameState(int slotNumber)
        {
            _slotNumber = slotNumber;
        }

        public override string GetSaveFileLocation()
        {
            return $"{Application.persistentDataPath}/devsave_{_slotNumber}.json";
        }

        public override void DeserialiseFromPersistence()
        {
            throw new NotImplementedException();
        }
    }
}
