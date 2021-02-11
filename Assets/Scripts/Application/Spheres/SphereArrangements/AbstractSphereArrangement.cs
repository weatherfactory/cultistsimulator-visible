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
        public virtual void AddNewSphereToArrangement(Sphere sphere, int index)
        {
            sphere.transform.SetParent(this.transform);
            sphere.transform.localScale = Vector3.one;
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localRotation = Quaternion.identity;
        }
    }



}
