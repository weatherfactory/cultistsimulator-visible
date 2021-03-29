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
using UnityEngine;

namespace SecretHistories.UI
{
    [IsEmulousEncaustable(typeof(AbstractDominion))]
    public class MinimalDominion: AbstractDominion
    {
        
        public MinimalDominion()
        {
            Identifier = SituationDominionEnum.Unknown.ToString();
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

        public override void Evoke()
        {
            //
        }

        public override void Dismiss()
        {
         //
        }

        public override bool CanCreateSphere(SphereSpec spec)
        {
            if (GetSphereById(spec.Id) != null)
                return false; //no spheres with duplicate id

            return true;
        }

        public override Sphere TryCreateSphere(SphereSpec spec)
        {
            if (!CanCreateSphere(spec))
                return NullSphere.Create();

            var newSphere = Watchman.Get<PrefabFactory>().InstantiateSphere(spec, _manifestable);
            newSphere.transform.SetParent(transform,false);
            _spheres.Add(newSphere);
            return newSphere;
        }

    }


}
