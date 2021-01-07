﻿using System;
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
  public  class TokenLocation
  {
      public Vector3 Position;
      public SpherePath AtSpherePath;
        public TokenLocation(float x,float y,float z, SpherePath atSpherePath)
        {
            Position.x = x;
            Position.y = y;
            Position.z = z;
            AtSpherePath = atSpherePath;

        }

        public TokenLocation(Vector3 position, Sphere inSphere)
        {
            Position = position;
            AtSpherePath = inSphere.GetPath();
        }


        public TokenLocation(Vector3 position,SpherePath atSpherePath)
        {
            Position = position;
            AtSpherePath = atSpherePath;
        }
        

        public TokenLocation(Token forToken)
        {
            Position = forToken.Location.Position;
            AtSpherePath = forToken.Sphere.GetPath();
        }


  }
}
