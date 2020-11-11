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
   public class SpherePath
   {
       private string _path;

       public const char SEPARATOR='_';

       public override string ToString()
       {
           return _path;
       }

       public bool Equals(SpherePath otherPath)
       {
           return otherPath.ToString() == _path;
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
