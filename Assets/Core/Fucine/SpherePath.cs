using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;
using Noon;

namespace Assets.Core.Fucine
{
   public class SpherePath:IEquatable<SpherePath>
   {
       public override bool Equals(object obj)
       {
           if (ReferenceEquals(null, obj)) return false;
           if (ReferenceEquals(this, obj)) return true;
           if (obj.GetType() != this.GetType()) return false;
           return Equals((SpherePath) obj);
       }

       public override int GetHashCode()
       {
           return (_path != null ? _path.GetHashCode() : 0);
       }

       private readonly string _path;

       public const char SEPARATOR='_';

       public override string ToString()
       {
           return _path;
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

           return otherPath?.ToString() == _path;
       }


       public SpherePath(string pathStringRepresentation)
       {
           _path = pathStringRepresentation;
           
       }

       public static SpherePath SituationPath(IVerb verb)
       {
           string situationPathString = verb.Id + SEPARATOR + DateTime.Now.ToString(CultureInfo.InvariantCulture);
           return new SpherePath(situationPathString);
       }

       public SpherePath(SpherePath originalPath, string pathStringRepresentation)
       {

           _path = originalPath.ToString() + SEPARATOR + pathStringRepresentation;

       }
   }
}
