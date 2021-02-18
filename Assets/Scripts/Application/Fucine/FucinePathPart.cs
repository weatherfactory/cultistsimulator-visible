using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Fucine
{
    public abstract class FucinePathPart
    {

        public enum PathCategory
        {
            Root,
            Token,
            Sphere,
            Null,
            Current
        }

        protected string PathId;
        public abstract PathCategory Category { get; }
       

        protected FucinePathPart(string pathId)
        {
            PathId = pathId;
        }

        public override string ToString()
        {
            return PathId;
        }
    }
}
