using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Fucine;
using Newtonsoft.Json;
using SecretHistories.Fucine;

namespace SecretHistories.Interfaces
{
    public class FucinePath
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
            return (PathParts.First().Category == FucinePathId.PathCategory.Root);
        }

        public SpherePath Sphere { get; }
    
        public TokenPath Token {get;}

        [JsonConstructor]
        public FucinePath(string path)
        {
            

       //  ./sphere1!situationa/sphere2!situationb
           List<string> sphereParts = path.Split(SPHERE).ToList();
          //  .
          //  sphere1!situationa
          //  sphere2!situationb
            

          if(sphereParts[0]==ROOT.ToString())
          {
            AddRootPart();
              sphereParts.RemoveAt(0);
          }
            foreach (var spherePart in sphereParts)
            {
                if(!string.IsNullOrEmpty(spherePart) && !string.IsNullOrWhiteSpace(spherePart))
                {
                    string[] mightBeSphereAndSituation = spherePart.Split(SITUATION);
                    if (mightBeSphereAndSituation.Length > 1)
                        AddSphereAndToken(mightBeSphereAndSituation[0],mightBeSphereAndSituation[1]);
                    else
                        AddSphere(mightBeSphereAndSituation[0]);
                }
            }
        }

        private void AddSphereAndToken(string sphere, string token)
        {
            SpherePathId spherePathId=new SpherePathId(sphere);
            PathParts.Add(spherePathId);
            TokenPathId tokenPathId=new TokenPathId(token);
            PathParts.Add(tokenPathId);
        }

        private void AddSphere(string sphere)
        {
            SpherePathId spherePathId=new SpherePathId(sphere);
            PathParts.Add(spherePathId);
        }

        
        public FucinePath(string path1, string path2)
        {
            throw new NotImplementedException();

        }

        public FucinePath(FucinePath existingPath, string appendPath)
        {
            throw new NotImplementedException();

        }

        public FucinePath(TokenPath existingPath, SpherePath appendPath)
        {
            throw new NotImplementedException();
        }

        private void AddRootPart()
        {
            PathParts.Add(new RootPathId());
        }

    }
}
