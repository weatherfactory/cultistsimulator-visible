using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Entities;
using UnityEngine;

namespace SecretHistories.UI
{
    //Locates a token via:
    // its sphere
    // its *local* position (specifically, anchored3D position) relative to that sphere
    public class TokenLocation
  {
      public Vector3 Anchored3DPosition;
      public SpherePath AtSpherePath;
        public TokenLocation(float x,float y,float z, SpherePath atSpherePath)
        {
            Anchored3DPosition.x = x;
            Anchored3DPosition.y = y;
            Anchored3DPosition.z = z;
            AtSpherePath = atSpherePath;

        }

        public TokenLocation(Vector3 anchored3DPosition, Sphere inSphere)
        {
            Anchored3DPosition = anchored3DPosition;
            AtSpherePath = inSphere.GetPath();
        }


        public TokenLocation(Vector3 anchored3DPosition,SpherePath atSpherePath)
        {
            Anchored3DPosition = anchored3DPosition;
            AtSpherePath = atSpherePath;
        }
        

        public TokenLocation(Token forToken)
        {
            Anchored3DPosition = forToken.Location.Anchored3DPosition;
            AtSpherePath = forToken.Sphere.GetPath();
        }


  }
}
