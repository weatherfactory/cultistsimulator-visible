using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Interfaces;

namespace SecretHistories.Fucine
{
    public class SituationPath: IEquatable<SituationPath>
    {
        public const char SEPARATOR = '_';

        private readonly string _path;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SpherePath)obj);
        }

        public override int GetHashCode()
        {
            return (_path != null ? _path.GetHashCode() : 0);
        }

        
        public static bool operator ==(SituationPath path1, SituationPath path2)
        {
            return path1.Equals(path2);
        }

        public static bool operator !=(SituationPath path1, SituationPath path2)
        {
            return !(path1 == path2);
        }

        public bool Equals(SituationPath otherPath)
        {

            return otherPath?.ToString() == _path;
        }
        public override string ToString()
        {
            return _path;
        }

        public SituationPath(IVerb verb)
        {
            _path = verb.Id + SEPARATOR + Guid.NewGuid();
        }

        public SituationPath(String path)
        {
            _path = path;
        }
    }
}
