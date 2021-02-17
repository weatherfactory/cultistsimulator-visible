using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecretHistories.Fucine;

namespace SecretHistories.Interfaces
{
    public abstract class FucinePath
    {
        public const char ROOT = '.';
        public const char SITUATION = '!'; 
        public const char SPHERE = '/';
        public const char CURRENT = '#';

        protected List<FucinePathId> PathParts=new List<FucinePathId>();

        public bool IsValid()
        {
            throw new NotImplementedException();
        }

        public bool IsAbsolute()
        {
            throw new NotImplementedException();
        }

        public abstract SpherePath Sphere { get; }
    
        public abstract TokenPath Token {get;}

        [JsonConstructor]
        protected FucinePath(string path)
        {
            throw new NotImplementedException();
        }

        protected FucinePath(string path1, string path2)
        {
            throw new NotImplementedException();

        }

        protected FucinePath(FucinePath existingPath, string appendPath)
        {
            throw new NotImplementedException();

        }

        protected FucinePath(TokenPath existingPath, SpherePath appendPath)
        {
            throw new NotImplementedException();
        }
    }
}
