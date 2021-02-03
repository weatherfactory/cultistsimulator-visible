using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Entities;
using SecretHistories.Interfaces;

namespace SecretHistories.Fucine
{
    public class SituationPath: FucinePath, IEquatable<SituationPath>
    {
        public const char SEPARATOR = '_';

        public string Path { get; private set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SpherePath)obj);
        }

        public override int GetHashCode()
        {
            return (Path != null ? Path.GetHashCode() : 0);
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

            return otherPath?.ToString() == Path;
        }
        public override string ToString()
        {
            return Path;
        }

        public override SituationPath GetBaseSituationPath()
        {
            return this;
        }

        public SituationPath(Verb verb)
        {
            Path = verb.Id + SEPARATOR + Guid.NewGuid();
        }

        [JsonConstructor]
        public SituationPath(String path)
        {
            Path = path;
        }

        public static SituationPath NullPath()
        {
            return new SituationPath("NULLPATH");
        }
    }
}
