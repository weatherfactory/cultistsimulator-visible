using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
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
        public const char WILD = '*';
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

        public virtual string DisplayStatus()
        {
            //could put more verbose descriptors in here
            return Validity.ToString();
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
                List<string> splitParts;
                //  ./sphere1!situationa/sphere2!situationb
                if (path[0]==ROOT)
                {
                    AddRootPart();
                    splitParts = path.Substring(1).Split(SPHERE).ToList();
                }
                else if (path[0] == WILD)
                {
                    AddWildPart();
                    splitParts = path.Substring(1).Split(SPHERE).ToList();
                }
                else
                {
                    splitParts = path.Split(SPHERE).ToList();
                }
                //  .
                //  sphere1!situationa
                //  sphere2!situationb

                if (splitParts.Any()) 
                {
                    ParseAndAddPartsSplitBySphere(splitParts);
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
            Validity = ValidateAfterParsing(PathParts);//This should use constructor chaining when I next refactor
        }

        public FucinePath(IEnumerable<FucinePathPart> parts)
        {
            PathParts.AddRange(parts);
            Validity = ValidateAfterParsing(PathParts); //This should use constructor chaining when I next refactor
        }


        public bool IsAbsolute()
        {
            return (PathParts.First().Category == FucinePathPart.PathCategory.Root);
        }
        public bool IsWild()
        {
            return (PathParts.First().Category == FucinePathPart.PathCategory.Wild);
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

        public static FucinePath Wild()
        {
            return new FucinePath(new WildPathPart());
        }

        private void ParseAndAddPartsSplitBySphere(List<string> partsSplitBySphereDivider)
        {
            foreach (var splitPart in partsSplitBySphereDivider)
            {

                if (!string.IsNullOrEmpty(splitPart) && !string.IsNullOrWhiteSpace(splitPart))
                {
                    if (splitPart.StartsWith(TOKEN.ToString()))
                        AddToken(splitPart); //it can't be followed by another /, because we've split those out. Add it as a token and move on.
                    else
                    {
                        if (splitPart.Contains(TOKEN.ToString()))
                        {
                            //This is a sphere followed by a token.
                            string[] splitOnTokenChar = splitPart.Split(TOKEN.ToString());
                            
                            AddSphereAndToken(splitOnTokenChar[0], splitOnTokenChar[1]);
                        }
                        else
                        {
                            //This is just a sphere.
                            AddSphere(splitPart);
                        }
                    }
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

        public FucinePath AppendingToken(string tokenId)
        {
            TokenPathPart tokenPathPart = new TokenPathPart(tokenId);
        var partsForNewPath=new List<FucinePathPart>(PathParts);
        partsForNewPath.Add(tokenPathPart);
            var newPath=new FucinePath(partsForNewPath);
            return newPath;
        }


        public FucinePath AppendingSphere(string sphereId)
        {
            SpherePathPart spherePathPart = new SpherePathPart(sphereId);
            var partsForNewPath = new List<FucinePathPart>(PathParts);
            partsForNewPath.Add(spherePathPart);
            var newPath = new FucinePath(partsForNewPath);
            return newPath;
        }



        public FucinePath AppendingPath(string appendPathString)
        {
          var pathToAppend=new FucinePath(appendPathString);
          return AppendingPath(pathToAppend);

        }

        public FucinePath AppendingPath(FucinePath pathToAppend)
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

        private void AddWildPart()
        {
            PathParts.Add(new WildPathPart());
        }

        public static FucinePath Current()
        {
            return new FucinePath(String.Empty);
        }

        public bool Conforms(FucinePath otherPath)
        {

     if (otherPath.IsWild())
         return ConformsWild(otherPath);

     return Equals(otherPath);
            
        }
        public bool ConformsWild(FucinePath wildPath)
        {

            if (!wildPath.IsWild())
            {
                NoonUtility.LogWarning($"Trying to wild-match {this} with a non-wild path: {wildPath}");
                return false;
            }


            if (PathParts.Count < wildPath.PathParts.Count)
                return false; //a wild path can have fewer parts than a matching absolute path, but the reverse is not true

            int thisPathIndex = PathParts.Count - 1;
            int wildPathIndex = wildPath.PathParts.Count - 1;


            while (wildPathIndex >= 0)
            {
                FucinePathPart wildPartToCompare = wildPath.PathParts[wildPathIndex];
                
                if (wildPartToCompare.Category != FucinePathPart.PathCategory.Wild)
                {
                    FucinePathPart thisPartToCompare = PathParts[thisPathIndex];
                    if (thisPartToCompare.Category != wildPartToCompare.Category)
                        return false;
                    if (thisPartToCompare.GetId() != wildPartToCompare.GetId())
                        return false;
                }

                wildPathIndex--;
                thisPathIndex--;
            }

            return true;

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
