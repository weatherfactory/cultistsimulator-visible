using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Fucine
{
    public abstract class FucinePathId
    {

        protected string _pathId;
       

        protected FucinePathId(string pathId)
        {

        }

        public override string ToString()
        {
            return _pathId;
        }
    }
}
