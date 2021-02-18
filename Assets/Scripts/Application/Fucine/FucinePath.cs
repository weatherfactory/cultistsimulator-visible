using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Fucine;
using Newtonsoft.Json;
using SecretHistories.Enums;
using SecretHistories.Fucine;

namespace SecretHistories.Fucine
{
    public class FucinePath
    {
        public const char ROOT = '.';
        public const char TOKEN = '!';
        public const char SPHERE = '/';
        public const char CURRENT = '#';



        protected List<FucinePathPart> PathParts = new List<FucinePathPart>();


        protected FucineValidity Validity = FucineValidity.Uninitialised;

        public virtual bool IsValid()
        {
            if (Validity == FucineValidity.Valid)
                return true;

            return false;
        }


    [JsonConstructor]
        public FucinePath(string path)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
            {
                Validity = FucineValidity.Empty;
                return;
            }

            try
            {
                List<string> sphereParts;

                if(path[0]==ROOT)
                {
                    AddRootPart();
                    sphereParts=path.Substring(1).Split(SPHERE).ToList();
                }
                else
                    //  ./sphere1!situationa/sphere2!situationb
                    sphereParts = path.Split(SPHERE).ToList();
                //  .
                //  sphere1!situationa
                //  sphere2!situationb

                if (sphereParts.Any()) 
                {
                    Parse(sphereParts);
                }

                Validity=Validate(PathParts);

            }
            catch (Exception e)
            {
                NoonUtility.Log($"Error parsing fucine path: {path}, ({e.Message})");
                Validity = FucineValidity.ParsingError;
            }


        }

        private FucineValidity Validate(List<FucinePathPart> pathParts)
        {
            if (!pathParts.Any())
                return  FucineValidity.Empty;

            if (pathParts.Count > 1)
            {
                if (pathParts[0].Category == FucinePathPart.PathCategory.Root &&
                    pathParts[1].Category == FucinePathPart.PathCategory.Token)
                    return FucineValidity.TokenInRoot;
            }

            return FucineValidity.Valid;
        }

        public FucinePath(FucinePathPart part)
        {
            PathParts.Add(part);
        }

        public FucinePath(IEnumerable<FucinePathPart> parts)
        {
            PathParts.AddRange(parts);
        }


        public bool IsAbsolute()
        {
            return (PathParts.First().Category == FucinePathPart.PathCategory.Root);
        }

        public FucinePathPart EndingPathPart
        {
            get
            {
                if(!PathParts.Any())
                    return new NullSpherePathPart();

                return PathParts.Last();
            }
        }

        public FucinePath SpherePath
        {
            get
            {
                if(!PathParts.Any())
                    return new NullFucinePath();

                if (EndingPathPart.Category == FucinePathPart.PathCategory.Sphere)
                    return this;
                //doesn't end with a sphere; it's either a root path...
                if(EndingPathPart.Category==FucinePathPart.PathCategory.Root)
                    return FucinePath.Root();
                //or something else we don't want. Iterate up the string until we find a sphere or run out of parts
                else
                {
                    var pathPartsRemaining = PathParts.Take(PathParts.Count - 1);
                    var subPath=new FucinePath(pathPartsRemaining);
                    return subPath.SpherePath;
                }
                
            }
        }

        public override string ToString()
        {
            return string.Join(string.Empty, PathParts);
        }

        public FucinePath TokenPath
        {
            get
            {
                if(!PathParts.Any())
                    return new NullFucinePath();

                if (EndingPathPart.Category == FucinePathPart.PathCategory.Token)
                    return this;
                if (EndingPathPart.Category == FucinePathPart.PathCategory.Root)
                    return FucinePath.Root();
                else
                {
                    var pathPartsRemaining = PathParts.Take(PathParts.Count - 1);
                    var subPath=new FucinePath(pathPartsRemaining);
                    return subPath.TokenPath;
                }

            }
        }

      

        public static FucinePath Root()
        {
            return new FucinePath(new RootPathPart());
        }

        private void Parse(List<string> sphereParts)
        {
       
                if (sphereParts[0].StartsWith(TOKEN.ToString()) && sphereParts.Count == 1)
                {
                    //this is a token path part, on its own. This is only legal when there is exactly one path part:
                    //it has to be relative (so no root) and it can't be preceded by anything else (or it would have to be preceded by a sphere
                    AddToken(sphereParts[0]);
                    return;
                }

                foreach (var spherePart in sphereParts)
                {

                    if (!string.IsNullOrEmpty(spherePart) && !string.IsNullOrWhiteSpace(spherePart))
                    {
                        string[] mightBeSphereAndSituation = spherePart.Split(TOKEN);
                        if (mightBeSphereAndSituation.Length > 1)
                            AddSphereAndToken(mightBeSphereAndSituation[0], mightBeSphereAndSituation[1]);
                        else
                            AddSphere(mightBeSphereAndSituation[0]);
                    }
                }
            

        }

        private void AddToken(string token)
        {
            TokenPathPart tokenPathPart=new TokenPathPart(token);
            PathParts.Add(tokenPathPart);
        }

        private void AddSphereAndToken(string sphere, string token)
        {
            SpherePathPart spherePathPart=new SpherePathPart(sphere);
            PathParts.Add(spherePathPart);
            TokenPathPart tokenPathPart=new TokenPathPart(token);
            PathParts.Add(tokenPathPart);
        }

        private void AddSphere(string sphere)
        {
            SpherePathPart spherePathPart=new SpherePathPart(sphere);
            PathParts.Add(spherePathPart);
        }

        
        public FucinePath(string path1, string path2)
        {
            throw new NotImplementedException();

        }

        public FucinePath(FucinePath existingPath, string appendPath)
        {
            throw new NotImplementedException();

        }

        public FucinePath(FucinePath existingPath, FucinePath appendPath)
        {
            throw new NotImplementedException();
        }

        private void AddRootPart()
        {
            PathParts.Add(new RootPathPart());
        }

        public static FucinePath Current()
        {
            return new FucinePath(CURRENT.ToString());
        }
    }
}
