using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using UnityEngine;

namespace SecretHistories.Infrastructure.Persistence
{
    public class DevSlotSaveGamePersistence: GamePersistence
    {
        private readonly int _slotNumber;

        public DevSlotSaveGamePersistence(int slotNumber)
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
