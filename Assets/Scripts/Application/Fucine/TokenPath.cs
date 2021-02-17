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
    public class TokenPath: FucinePath, IEquatable<TokenPath>
    {
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

        
        public static bool operator ==(TokenPath path1, TokenPath path2)
        {
            return path1.Equals(path2);
        }

        public static bool operator !=(TokenPath path1, TokenPath path2)
        {
            return !(path1 == path2);
        }

        public bool Equals(TokenPath otherPath)
        {

            return otherPath?.ToString() == Path;
        }
        public override string ToString()
        {
            return Path;
        }

        public override bool IsAbsolute()
        {
            return false;
        }

        public override SpherePath Sphere { get; }
        public override TokenPath Token { get; }

        public TokenPath(Verb verb)
        {
            Path = verb.Id + SPHERE + Guid.NewGuid();
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Path))
                return false;
            return true;
        }

        [JsonConstructor]
        public TokenPath(String path)
        {
            Path = path;

        }

        public static TokenPath Root()
        {
            StringBuilder rootPath=new StringBuilder();

            rootPath.Append(new char[] {SITUATION, FucinePath.ROOT});

            return new TokenPath(rootPath.ToString());
        }
    }
}
