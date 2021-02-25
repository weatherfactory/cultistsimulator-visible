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

        protected string PathPartValue;
        public abstract PathCategory Category { get; }

        public abstract string GetId();

        protected FucinePathPart(string pathPartValue)
        {
            PathPartValue = pathPartValue;
        }

        public override string ToString()
        {
            return PathPartValue;
        }
    }
}
