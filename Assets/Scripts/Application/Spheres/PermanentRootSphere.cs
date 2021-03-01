using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres
{
    public class PermanentRootSphere: MonoBehaviour
    {
    public string ApplyId;
        
        private SphereSpec _sphereSpec;

        public void Awake()
        {
            //registering awake on hardcoded spheres ONLY using this approach.
            //when we call it on Awake on all spheres, then instantiated spheres get registered on instantiation, before their spec is applied.
            var permanentRootSphere = gameObject.GetComponent<Sphere>();
            if(permanentRootSphere==null)
                NoonUtility.LogWarning("ERP: Editable sphere spec without any discernible sphere on game object " + gameObject.name);

        ApplySpecToSphere(permanentRootSphere);
        }
        public void ApplySpecToSphere(Sphere applyToSphere)
        {
            
            if(string.IsNullOrEmpty(ApplyId))
                NoonUtility.LogWarning("SpecApplier for sphere " + applyToSphere.name + " doesn't have an id specified.");
            _sphereSpec=new SphereSpec(applyToSphere.GetType(), ApplyId);

            applyToSphere.ApplySpec(_sphereSpec);

            FucineRoot.Get().AddSphere(applyToSphere);

        }
    }
}
