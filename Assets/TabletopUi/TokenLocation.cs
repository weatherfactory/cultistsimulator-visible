using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.TabletopUi.Scripts.Infrastructure;
using UnityEngine;

namespace Assets.TabletopUi
{
  public  class TokenLocation
  {
      public Vector3 Position;
      public TokenContainer InContainer;
        public TokenLocation(float x,float y,float z)
        {
            Position.x = x;
            Position.y = y;
            Position.z = z;
        }

        public TokenLocation(Vector2 position)
        {
            Position = position;
        }

        public TokenLocation(Vector3 position)
        {
            Position = position;
        }

        public TokenLocation(Vector3 position,TokenContainer container)
        {
            Position = position;
            InContainer = container;
        }

    }
}
