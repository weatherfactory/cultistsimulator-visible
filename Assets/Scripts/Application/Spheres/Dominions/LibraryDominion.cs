using System;
using System.Collections;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Dominions
{
    public class LibraryDominion : AbstractDominion
    {
        [SerializeField] private string EditableIdentifier;


        public override void Awake()
        {
            Identifier = EditableIdentifier;
            var roomSpheres = GetComponentsInChildren<RoomSphere>();
            foreach (var r in roomSpheres)
            {
                var roomSphereSpec = r.gameObject.GetComponent<PermanentSphereSpec>();
                roomSphereSpec.ApplySpecToSphere(r);
                r.SetContainer(FucineRoot.Get());
                Watchman.Get<HornedAxe>().RegisterSphere(r);
            }

            base.Awake();
        }

        

        public override Sphere TryCreateSphere(SphereSpec spec)
        {
            throw new NotImplementedException();
        }

        public override bool VisibleFor(string state)
        {
            throw new NotImplementedException();
        }

        public override bool RelevantTo(string state, Type sphereType)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveSphere(string id, SphereRetirementType retirementType)
        {
            throw new NotImplementedException();
        }

        public override bool CanCreateSphere(SphereSpec spec)
        {
            throw new NotImplementedException();
        }
    }
}