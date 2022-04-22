using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Dominions
{
    [IsEmulousEncaustable(typeof(AbstractDominion))]
    public class AnnexDominion: AbstractDominion

    {
        public override Sphere TryCreateOrRetrieveSphere(SphereSpec spec)
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
    }
}
