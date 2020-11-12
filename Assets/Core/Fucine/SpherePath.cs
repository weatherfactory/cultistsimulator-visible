using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Interfaces;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;

namespace Assets.Core.Fucine
{
   public class SpherePath:IEquatable<SpherePath>
   {
       public const char SEPARATOR = '_';
       private readonly string _path;
       private readonly string _sphereIdentifier;
       public SituationPath SituationPath { get; protected set; }

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


       public SpherePath(string sphereIdentifier)
       {
           _sphereIdentifier = sphereIdentifier;
           _path = sphereIdentifier;
           
       }

       public SpherePath(SituationPath situationPath, string sphereIdentifier)
       {
           SituationPath = situationPath;
           _sphereIdentifier = sphereIdentifier;
           _path = SituationPath.ToString() + SEPARATOR + sphereIdentifier;
       }

       public SpherePath(SpherePath originalPath, string sphereIdentifier)
       {

           _path = originalPath.ToString() + SEPARATOR + sphereIdentifier;

       }
   }
}
