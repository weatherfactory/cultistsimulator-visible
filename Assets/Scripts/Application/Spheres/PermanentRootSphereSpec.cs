using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Spheres
{
    public class PermanentRootSphereSpec: PermanentSphereSpec
    {
        
        public string EnRouteSpherePath;
        public string WindowsSpherePath;

        public void Awake()
        {
            //registering awake on permanent root spheres ONLY using this approach.
            //when we call it on Awake on all spheres, then instantiated spheres get registered on instantiation, before their spec is applied.
            //non-root permanent spheres have their spec applied by their parent dominion.
            ApplySpecToSphere(GetSphereComponent());
        }

        public override void ApplySpecToSphere(Sphere applyToSphere)
        {
            
            if(string.IsNullOrEmpty(ApplyId))
                NoonUtility.LogWarning("SpecApplier for sphere " + applyToSphere.name + " doesn't have an id specified.");

            _sphereSpec=new SphereSpec(applyToSphere.GetType(), ApplyId);
            _sphereSpec.EnRouteSpherePath=new FucinePath(EnRouteSpherePath);
            _sphereSpec.WindowsSpherePath = new FucinePath(WindowsSpherePath);

            applyToSphere.SetPropertiesFromSpec(_sphereSpec);

            FucineRoot.Get().AttachSphere(applyToSphere);
            Watchman.Get<HornedAxe>().RegisterSphere(applyToSphere);
        }
    }
}
