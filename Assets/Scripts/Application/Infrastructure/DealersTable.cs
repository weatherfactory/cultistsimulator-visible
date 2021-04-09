using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Logic;
using Assets.Scripts.Application.Abstract;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Infrastructure
{
    [IsEmulousEncaustable(typeof(AbstractDominion))]
    public class DealersTable: AbstractDominion, IHasCardPiles
    {
        
        [SerializeField] private string EditableIdentifier;

        
        public IHasAspects Container=>FucineRoot.Get(); //in case we ever move it

        public void Awake()
        {
            Identifier = EditableIdentifier;
            var w=new Watchman();
            w.Register(this);
        }

        public IEnumerable<IHasElementTokens> GetDrawPiles()
        {
            var drawDeckSpheres = _spheres.Where(s => s.GoverningSphereSpec.SphereType == typeof(DrawPile));
            return drawDeckSpheres;
        }

        public IHasElementTokens GetDrawPile(string forDeckSpecId)
        {
            return _spheres.SingleOrDefault(s =>
                s.GoverningSphereSpec.ActionId == forDeckSpecId && s.GoverningSphereSpec.SphereType==typeof(DrawPile));
        }

        public IHasElementTokens GetForbiddenPile(string forDeckSpecId)
        {
            return _spheres.SingleOrDefault(s =>
                s.GoverningSphereSpec.ActionId == forDeckSpecId && s.GoverningSphereSpec.SphereType == typeof(ForbiddenPile));
        }


        public override Sphere TryCreateSphere(SphereSpec spec)
        {
            var newSphere=Watchman.Get<PrefabFactory>().InstantiateSphere(spec, Container);
            newSphere.transform.SetParent(transform);
            _spheres.Add(newSphere);

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
            throw new NotImplementedException();
        }


        public override bool CanCreateSphere(SphereSpec spec)
        {
            return true;
        }

    }
}
