using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Infrastructure.Persistence
{
    public class NewGame: PersistedGame
    {
        public override string GetSaveFileLocation()
        {
            return string.Empty;
        }

        public override bool Exists()
        {
            return false;
        }

    }
}
