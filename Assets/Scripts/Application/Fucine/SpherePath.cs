using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SecretHistories.Elements;
using SecretHistories.Interfaces;
using SecretHistories.NullObjects;

namespace SecretHistories.Fucine
{
   public class SpherePath: FucinePath
   {

    
        [JsonConstructor]
       public SpherePath(string path):base(path)
       {
       }

       public SpherePath(string path1, string path2) : base(path1, path2)
       {
           throw new NotImplementedException();

       }

       public SpherePath(FucinePath existingPath, string appendPath) : base(existingPath, appendPath)
       {
           throw new NotImplementedException();

       }

       public SpherePath(TokenPath tPath, SpherePath sPath): base(tPath,sPath)
       {
           throw new NotImplementedException();
       }


        public static SpherePath Current()
       { 
           return new SpherePath(CURRENT.ToString());
       }

   
   }
}
