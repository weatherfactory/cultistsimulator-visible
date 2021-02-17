using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Fucine
{
    public abstract class FucinePathId
    {

        public enum PathCategory
        {
            Root,
            Token,
            Sphere
        }

        protected string _pathId;
        public abstract PathCategory Category { get; }
       

        protected FucinePathId(string pathId)
        {

        }

        public override string ToString()
        {
            return _pathId;
        }
    }
}
