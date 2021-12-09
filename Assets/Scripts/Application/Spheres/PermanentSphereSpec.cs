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
    public class PermanentSphereSpec : MonoBehaviour
    {
        public string ApplyId;
   
        protected SphereSpec _sphereSpec;

        public Sphere GetSphereComponent()
        {
            var applyToSphere = gameObject.GetComponent<Sphere>();
            if (applyToSphere == null)
                NoonUtility.LogWarning("ERP: Editable sphere spec without any discernible sphere on game object " + gameObject.name);

            return applyToSphere;
        }


        public virtual void ApplySpecToSphere(Sphere applyToSphere)
        {

            if (string.IsNullOrEmpty(ApplyId))
                NoonUtility.LogWarning("SpecApplier for sphere " + applyToSphere.name + " doesn't have an id specified.");

            _sphereSpec = new SphereSpec(applyToSphere.GetType(), ApplyId);
            applyToSphere.SetPropertiesFromSpec(_sphereSpec);
            
            Watchman.Get<HornedAxe>().RegisterSphere(applyToSphere);
        }
    }
}
