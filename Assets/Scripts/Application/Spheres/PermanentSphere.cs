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
    public class PermanentSphere : MonoBehaviour
    {
        public string ApplyId;

        protected SphereSpec _sphereSpec;
        

        public void Awake()
        {
            //registering awake on permanent root spheres ONLY using this approach.
            //when we call it on Awake on all spheres, then instantiated spheres get registered on instantiation, before their spec is applied.
            var applyToSphere = gameObject.GetComponent<Sphere>();
            if (applyToSphere == null)
                NoonUtility.LogWarning("ERP: Editable sphere spec without any discernible sphere on game object " + gameObject.name);

            ApplySpecToSphere(applyToSphere);
        }
        protected virtual void ApplySpecToSphere(Sphere applyToSphere)
        {

            if (string.IsNullOrEmpty(ApplyId))
                NoonUtility.LogWarning("SpecApplier for sphere " + applyToSphere.name + " doesn't have an id specified.");

            _sphereSpec = new SphereSpec(applyToSphere.GetType(), ApplyId);

            applyToSphere.ApplySpec(_sphereSpec);

            Watchman.Get<HornedAxe>().RegisterSphere(applyToSphere);
        }
    }
}
