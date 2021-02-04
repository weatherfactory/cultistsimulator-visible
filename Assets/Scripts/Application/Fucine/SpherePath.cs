using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SecretHistories.Elements;
using SecretHistories.Interfaces;
using SecretHistories.NullObjects;

namespace SecretHistories.Fucine
{
   public class SpherePath: FucinePath,IEquatable<SpherePath>
   {
       public const char SEPARATOR = '_';
       public const char CURRENT = '@';

       public string Path { get; private set; }

        public override bool Equals(object obj)
       {
           if (ReferenceEquals(null, obj)) return false;
           if (ReferenceEquals(this, obj)) return true;
           if (obj.GetType() != this.GetType()) return false;
           return Equals((SpherePath) obj);
       }

       public override int GetHashCode()
       {
           return (Path != null ? Path.GetHashCode() : 0);
       }

       

       public override string ToString()
       {
           return Path;
       }

       public override SituationPath GetBaseSituationPath()
       {
           List<string> parts = new List<string>(Path.Split(SEPARATOR));
           if (parts.Count == 1)
               return SituationPath.NullPath();
           return new SituationPath(parts.First());
       }

       public static bool operator ==(SpherePath path1, SpherePath path2)
       {
           return path1.Equals(path2);
       }

       public static bool operator !=(SpherePath path1, SpherePath path2)
       {
           return !(path1 == path2);
       }

       public bool Equals(SpherePath otherPath)
       {

           return otherPath?.ToString() == Path;
       }

        [JsonConstructor]
       public SpherePath(string path)
       {
           Path = path;
       }


        public SpherePath(FucinePath basePath, string sphereIdentifier)
        {
            Path = basePath.ToString() + SEPARATOR + sphereIdentifier;
       }

       
       public static SpherePath Current()
       { 
           return new SpherePath(CURRENT.ToString());
       }
   }
}
