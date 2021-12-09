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

    public class FucinePath:IEquatable<FucinePath>
    {
        public const char ROOT = '~';
        public const char TOKEN = '!';
        public const char SPHERE = '/';


        protected List<FucinePathPart> PathParts = new List<FucinePathPart>();


        protected FucineValidity Validity = FucineValidity.Uninitialised;

        public virtual bool IsValid()
        {
            if (Validity == FucineValidity.Valid)
                return true;

            return false;
        }

        public virtual bool IsEmpty()
        {
            if (Validity == FucineValidity.Empty)
                return true;

            return false;
        }

        public override string ToString()
        {
            return string.Join(string.Empty, PathParts);
        }

        public string Path => ToString();
        
        [JsonConstructor]
        public FucinePath(string path)
        {
            if (path == null)
            {
                AddNullPathPart();
                Validity = FucineValidity.Empty;
                return;
            }


            path = path.Trim(); //remove whitespace
            if (path==string.Empty)
            {
                //changed our mind. Empty path is no longer valid.
                AddNullPathPart();
                Validity = FucineValidity.Empty;
                return;
            }

            try
            {
                List<string> sphereParts;
                //  ./sphere1!situationa/sphere2!situationb
                if (path[0]==ROOT)
                {
                    AddRootPart();
                    sphereParts=path.Substring(1).Split(SPHERE).ToList();
                }
                else
                {
                    AddCurrentLocationPart();   
                    sphereParts = path.Split(SPHERE).ToList();
                }
                //  .
                //  sphere1!situationa
                //  sphere2!situationb

                if (sphereParts.Any()) 
                {
                    Parse(sphereParts);
                }

                Validity=ValidateAfterParsing(PathParts);

            }
            catch (Exception e)
            {
              throw new ApplicationException($"Error parsing Fucine path: {path}, ({e.Message})",e);
            }


        }

        private FucineValidity ValidateAfterParsing(List<FucinePathPart> pathParts)
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

        public FucinePathPart GetEndingPathPart()
        {
            if (!PathParts.Any())
                return new NullFucinePathPart();

            return PathParts.Last();
        }

        public FucinePath GetSpherePath()
        {
            if (!PathParts.Any())
                return new NullFucinePath();

            if (GetEndingPathPart().Category == FucinePathPart.PathCategory.Sphere)
                return this;
            //doesn't end with a sphere; it's either a root path...
            if (GetEndingPathPart().Category == FucinePathPart.PathCategory.Root)
                return FucinePath.Root();
            //or something else we don't want. Iterate up the string until we find a sphere or run out of parts
            else
            {
                var pathPartsRemaining = PathParts.Take(PathParts.Count - 1);
                var subPath = new FucinePath(pathPartsRemaining);
                return subPath.GetSpherePath();
            }
        }



        public FucinePath GetTokenPath()
        {
            if (!PathParts.Any())
                return new NullFucinePath();

            if (GetEndingPathPart().Category == FucinePathPart.PathCategory.Token)
                return this;
            if (GetEndingPathPart().Category == FucinePathPart.PathCategory.Root)
                return FucinePath.Root();
            else
            {
                var pathPartsRemaining = PathParts.Take(PathParts.Count - 1);
                var subPath = new FucinePath(pathPartsRemaining);
                return subPath.GetTokenPath();
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

        public FucinePath AppendToken(string tokenId)
        {
            TokenPathPart tokenPathPart = new TokenPathPart(tokenId);
        var partsForNewPath=new List<FucinePathPart>(PathParts);
        partsForNewPath.Add(tokenPathPart);
            var newPath=new FucinePath(partsForNewPath);
            return newPath;
        }


        public FucinePath AppendSphere(string sphereId)
        {
            SpherePathPart spherePathPart = new SpherePathPart(sphereId);
            var partsForNewPath = new List<FucinePathPart>(PathParts);
            partsForNewPath.Add(spherePathPart);
            var newPath = new FucinePath(partsForNewPath);
            return newPath;
        }



        public FucinePath AppendPath(string appendPathString)
        {
          var pathToAppend=new FucinePath(appendPathString);
          return AppendPath(pathToAppend);

        }

        public FucinePath AppendPath(FucinePath pathToAppend)
        {
            if(pathToAppend.IsAbsolute() && this.IsAbsolute())
                throw new InvalidOperationException($"Can't combine two absolute paths: '{this.ToString()}' and '{pathToAppend.ToString()}'");

            string newPathString= string.Concat(this.ToString(),pathToAppend.ToString());

            var newPath = new FucinePath(newPathString);
            return newPath;
        }

        private void AddNullPathPart()
        {
            PathParts.Add(new NullFucinePathPart());
        }

        private void AddRootPart()
        {
            PathParts.Add(new RootPathPart());
        }

        private void AddCurrentLocationPart()
        {
            PathParts.Add(new CurrentLocationPathPart());
        }

        public static FucinePath Current()
        {
            return new FucinePath(String.Empty);
        }

        public bool Equals(FucinePath other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return (this.ToString() == other.ToString());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FucinePath) obj);
        }

        public override int GetHashCode()
        {
            return (PathParts != null ? PathParts.GetHashCode() : 0);
        }

        public static bool operator ==(FucinePath left, FucinePath right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FucinePath left, FucinePath right)
        {
            return !Equals(left, right);
        }

        public bool IsPathToSphereInRoot()
        {
            if(PathParts.Count!=2) //it's empty, or it's just the root, or it's sphere + token, or....
                return false;

            if (PathParts.First().Category == FucinePathPart.PathCategory.Root &&
                PathParts[1].Category == FucinePathPart.PathCategory.Sphere)
                return true;

            return false;
        }
    }
}
