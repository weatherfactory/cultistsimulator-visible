using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.UI
{
    [IsEmulousEncaustable(typeof(AbstractDominion))]
    public class ShelfDominion: AbstractDominion
    {
     
        public override void RegisterFor(IManifestable manifestable)
        {
            _manifestable = manifestable;
            manifestable.RegisterDominion(this);

            for (int i = 0; i < 3; i++)
            {
                var shelfSpaceSpec=new SphereSpec(typeof(ShelfSpaceSphere),"shelf" + i);
               _spheres.Add(TryCreateSphere(shelfSpaceSpec));
            }
        }

        public override Sphere TryCreateSphere(SphereSpec spec)
        {
            if (!CanCreateSphere(spec))
                return NullSphere.Create();

            var newSphere=Watchman.Get<PrefabFactory>().InstantiateSphere(spec);
            newSphere.gameObject.transform.SetParent(this.gameObject.transform); //MANUALLY? really? the problem is that the gameobject hierarchy is now quite different from the FucinePath one
            OnSphereAdded.Invoke(newSphere);
            return newSphere;
        }


        public override bool VisibleFor(string state)
        {
            return true;
        }

        public override bool RelevantTo(string state, Type sphereType)
        {
            return true;
        }

        public override bool RemoveSphere(string id,SphereRetirementType retirementType)
        {
            var sphereToRemove = GetSphereById(id);
            if (sphereToRemove != null)
            {
                _spheres.Remove(sphereToRemove);
                OnSphereRemoved.Invoke(sphereToRemove);
                sphereToRemove.Retire(retirementType);
                return true;
            }
            else
                return false;
        }



        public override bool CanCreateSphere(SphereSpec spec)
        {
            return true;
        }
    }
}
