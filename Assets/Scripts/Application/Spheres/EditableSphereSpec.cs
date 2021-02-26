using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres
{
    public class EditableSphereSpec: MonoBehaviour
    {
     
        public string ApplyId;
        private SphereSpec _sphereSpec;
        

        public void ApplySpecToSphere(Sphere applyToSphere)
        {
            if(string.IsNullOrEmpty(ApplyId))
                NoonUtility.LogWarning("SpecApplier for sphere " + applyToSphere.name + " doesn't have an id specified.");
            _sphereSpec=new SphereSpec(applyToSphere.GetType(),ApplyId);

            applyToSphere.ApplySpec(_sphereSpec);
        }
    }
}
