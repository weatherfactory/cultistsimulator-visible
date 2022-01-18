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
            Wild,
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

        public string TrimTokenPrefix()
        {
            if (Category != PathCategory.Token)
                return PathPartValue;
            return PathPartValue.Substring(1);
        }

        public string TrimSpherePrefix()
        {
            if (Category != PathCategory.Sphere)
                return PathPartValue;
            return PathPartValue.Substring(1);
        }
    }
}
