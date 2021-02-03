using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;

namespace SecretHistories.Infrastructure.Persistence
{
    public class NewGame: GamePersistence
    {
        public override string GetSaveFileLocation()
        {
            return string.Empty;
        }

        public override bool Exists()
        {
            return false;
        }

        public override void DeserialiseFromPersistence()
        {
            throw new NotImplementedException();
        }
    }
}
