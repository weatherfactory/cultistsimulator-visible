using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.UI
{
    public class FitSphereToParentArrangement: AbstractSphereArrangement
    {
        public override void AddNewSphereToArrangement(Sphere newSphere, int index)
        {
          base.AddNewSphereToArrangement(newSphere, index);

          var newSphereRectTransform = newSphere.GetRectTransform();
          newSphereRectTransform.anchorMin=new Vector2(0,0);
          newSphereRectTransform.anchorMax = new Vector2(1, 1);
          newSphereRectTransform.offsetMin = new Vector2(0, 0);
            newSphereRectTransform.offsetMax=new Vector2(0,0);

        }


        public override void SphereRemoved(Sphere sphere)
        {
            //
        }
    }
}
