using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.UI
{
    //Locates a token via:
    // its sphere
    // its *local* position (specifically, anchored3D position) relative to that sphere
    public class TokenLocation
  {
      public virtual Vector3 Anchored3DPosition { get; set; }
      public FucinePath AtSpherePath { get; set; }

        public TokenLocation(){}

        public TokenLocation(float x,float y,float z, FucinePath atSpherePath)
        {
            Vector3 anchored3DPosition=new Vector3(x,y,z);
            Anchored3DPosition = anchored3DPosition;
            AtSpherePath = atSpherePath;

        }

        public TokenLocation(Vector3 anchored3DPosition, Sphere inSphere)
        {
            Anchored3DPosition = anchored3DPosition;
            AtSpherePath = inSphere.GetAbsolutePath();
        }


        public static TokenLocation Default(FucinePath atSpherePath)
        {
            return new TokenLocation(0,0,0, atSpherePath);
        }


        public TokenLocation(Vector3 anchored3DPosition, FucinePath atSpherePath)
        {
            Anchored3DPosition = anchored3DPosition;
            AtSpherePath = atSpherePath;
        }
        

        public TokenLocation(Token currentLocationOfToken)
        {
            Anchored3DPosition = currentLocationOfToken.Location.Anchored3DPosition;
            AtSpherePath = currentLocationOfToken.Sphere.GetAbsolutePath();
        }

        public TokenLocation WithSpherePath(FucinePath withSpherePath)
        {
            var newTl=new TokenLocation(Anchored3DPosition,withSpherePath);
            return newTl;
        }

        public TokenLocation WithSphere(Sphere withSphere)
        {
            return WithSpherePath(withSphere.GetAbsolutePath());
        }

        public bool HasValidSpherePath()
        {
            if (AtSpherePath == null)
                return false;
            if (!AtSpherePath.IsValid())
                return false;

            return true;
        }



  }
}
