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
      public Vector3 Anchored3DPosition { get; private set; }
      public SpherePath AtSpherePath { get; private set; }
        public TokenLocation(float x,float y,float z, SpherePath atSpherePath)
        {
            Vector3 anchored3DPosition=new Vector3(x,y,z);
            Anchored3DPosition = anchored3DPosition;
            AtSpherePath = atSpherePath;

        }

        public TokenLocation(Vector3 anchored3DPosition, Sphere inSphere)
        {
            Anchored3DPosition = anchored3DPosition;
            AtSpherePath = inSphere.GetPath();
        }

        public static TokenLocation Default()
        {
            return new TokenLocation(0,0,0,SpherePath.Current());
        }

        public TokenLocation(Vector3 anchored3DPosition,SpherePath atSpherePath)
        {
            Anchored3DPosition = anchored3DPosition;
            AtSpherePath = atSpherePath;
        }
        

        public TokenLocation(Token currentLocationOfToken)
        {
            Anchored3DPosition = currentLocationOfToken.Location.Anchored3DPosition;
            AtSpherePath = currentLocationOfToken.Sphere.GetPath();
        }

        public TokenLocation WithSpherePath(SpherePath withSpherePath)
        {
            var newTL=new TokenLocation(Anchored3DPosition,withSpherePath);
            return newTL;
        }

        public TokenLocation WithSphere(Sphere withSphere)
        {
            return WithSpherePath(withSphere.GetPath());
        }



  }
}
