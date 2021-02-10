using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.UI

{
    public abstract class AbstractSphereArrangement : MonoBehaviour
    {
        public abstract void ArrangeSphere(Sphere sphere, int index);
    }

    //we can't use an interface if we want it to be a serializable field
    public class SimpleSphereArrangement: AbstractSphereArrangement
    {
        public override void ArrangeSphere(Sphere sphere, int index)
        {
            sphere.transform.SetParent(this.transform);
            sphere.transform.localScale = Vector3.one;
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localRotation = Quaternion.identity;
        }
    }


}
