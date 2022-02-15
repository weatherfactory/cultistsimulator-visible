using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Elements;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.NullObjects;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace Assets.Scripts.Application.Entities.NullEntities
{
    public class NullSphere : Sphere
    {
        public override SphereCategory SphereCategory => SphereCategory.Null;

        private static NullSphere _instance;

        public override void Retire(SphereRetirementType rvfx)
        {
            Defunct = true;
        }

        public override bool IsValid => false;

        public static NullSphere Create()
        {
            if (_instance == null)
            {
                var obj = new GameObject("NullSphere");
                var nullSphereComponent = obj.AddComponent<NullSphere>();
                _instance = nullSphereComponent;
                _instance.SetPropertiesFromSpec(new SphereSpec(typeof(NullSphere),nameof(NullSphere)));

            }

            return _instance;
        }
    }
}
