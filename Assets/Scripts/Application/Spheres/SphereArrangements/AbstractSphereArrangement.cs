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
        public virtual void AddNewSphereToArrangement(Sphere newSphere, int index)
        {
            newSphere.transform.SetParent(this.transform);
            newSphere.transform.localScale = Vector3.one;
            newSphere.transform.localPosition = Vector3.zero;
            newSphere.transform.localRotation = Quaternion.identity;
        }
    }



}
