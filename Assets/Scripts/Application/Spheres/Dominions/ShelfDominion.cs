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
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.UI
{
    [IsEmulousEncaustable(typeof(AbstractDominion))]
    public class ShelfDominion: AbstractDominion
    {

        
        public override Sphere TryCreateOrRetrieveSphere(SphereSpec spec)
        {
            var existingSphere = _spheres.SingleOrDefault(s => s.Id == spec.Id);
            if (existingSphere != null)
                return existingSphere;


            NoonUtility.LogWarning($"Can't find a sphere in {_manifestable.Id} shelfdominion TryCreateOrRetrieveSphere that matches spec with id {spec.Id}. Returning null, which may cause trouble");
            return null;
        }


        public override bool VisibleFor(string state)
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
