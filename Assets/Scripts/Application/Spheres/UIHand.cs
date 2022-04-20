using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Spheres
{
 [IsEmulousEncaustable(typeof(Sphere))]
    public class UIHand: Sphere
    {
        public override SphereCategory SphereCategory => SphereCategory.World;
        public override float TokenHeartbeatIntervalMultiplier => 1;
        public override bool AllowDrag
        {
            get
            {
                return true;
            }
        }

        public override Vector3 TransformPositionInSphereToWorldSpace(Vector3 positionToTransform)
        {
            //This is a slight convenience, but also needs to be overridden for screen space spheres which we don't want to honour the z coord for - because it makes the 
            //token get up in the camera's business and look briefly enormous

            var UIPosition = Camera.main.ScreenToWorldPoint(positionToTransform);
            //      position.z = (thisCanvas.transform.position - Camera.main.transform.position).magnitude;
           
      
            //This is all pretty screwed. It needs a rethink, so we can actually drag the cards to the hand.


            return UIPosition;
        }

    }
}
