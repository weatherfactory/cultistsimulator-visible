using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;

namespace Assets.Scripts.Application.Entities.NullEntities
{
    public class NullLegacy: Legacy
    {
        public virtual bool IsValid()
        {
            return false;
        }
    }
}
