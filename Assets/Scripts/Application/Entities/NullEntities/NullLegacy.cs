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
        private static NullLegacy _instance;
        protected NullLegacy()
        {
 
        }
        public override bool IsValid()
        {
            return false;
        }
        public static NullLegacy Create()
        {
            if(_instance==null)
                _instance=new NullLegacy();
            

            return _instance;
        }
    }
}
