using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using SecretHistories.Entities;
using SecretHistories.UI;
using Vector3 = UnityEngine.Vector3;

namespace SecretHistories.Assets.Scripts.Application.Tokens
{
    /// <summary>
    /// Jon Postel: "Be conservative in what you do, be liberal in what you accept from others."
    /// </summary>
  public class TokenSuggestedLocation: TokenLocation
    {
        private Vector3 _anchored3DPosition;

        public override Vector3 Anchored3DPosition
        {
            get
            {
                var targetSphere = Watchman.Get<HornedAxe>().GetSphereByPath(AtSpherePath);
               

                return _anchored3DPosition;
            }
            set => _anchored3DPosition = value;
        }
    }
}
