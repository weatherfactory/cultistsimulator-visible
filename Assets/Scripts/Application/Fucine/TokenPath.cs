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
    public class TokenPath: FucinePath
    {



        public override SpherePath Sphere { get; }
        public override TokenPath Token { get; }


    

        [JsonConstructor]
        public TokenPath(String path): base(path)
        {

        }

        public TokenPath(string path1, string path2): base(path1,path2)
        {
            throw new NotImplementedException();

        }

        public TokenPath(FucinePath existingPath, string appendPath): base(existingPath, appendPath)
        {
            throw new NotImplementedException();

        }


        public static TokenPath Root()
        {
            StringBuilder rootPath=new StringBuilder();

            rootPath.Append(new char[] {SITUATION, FucinePath.ROOT});

            return new TokenPath(rootPath.ToString());
        }
    }
}
