using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Dominions
{
    [IsEmulousEncaustable(typeof(AbstractDominion))]
    public class WorldDominion : AbstractDominion
    {


        public override void Awake()
        {
            Identifier = gameObject.name;
            var spheres = GetComponentsInChildren<Sphere>();
            foreach (var s in spheres)
            {
                var sphereSpec = s.gameObject.GetComponent<PermanentSphereSpec>();
                if(sphereSpec==null)
                    NoonUtility.LogWarning($"Trying to set up spheres in a world dominion, but can't find a PermanentSphereSpec on {s.name}, so won't add it.");
                else
                {
                    
                    sphereSpec.ApplySpecToSphere(s);
                    s.SetContainer(FucineRoot.Get());
                    Watchman.Get<HornedAxe>().RegisterSphere(s);

                }
            }

            base.Awake();
        }

   

        public override Sphere TryCreateSphere(SphereSpec spec)
        {
            var existingSphere = _spheres.SingleOrDefault(s => s.Id == spec.Id);
            return existingSphere; 
        }

        public override bool VisibleFor(string state)
        {
            return true;
        }

        public override bool RelevantTo(string state, Type sphereType)
        {
            return true;
        }

        public override bool RemoveSphere(string id, SphereRetirementType retirementType)
        {
            throw new NotImplementedException(); //maybe later
        }

        public override bool CanCreateSphere(SphereSpec spec)
        {
            return false; //maybe later.
        }
    }
}