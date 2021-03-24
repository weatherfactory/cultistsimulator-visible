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
    [IsEncaustableClass(typeof(PopulateDominionCommand))]
    public class DealersTable: MonoBehaviour,IDominion
    {
        private List<Sphere> _spheres=new List<Sphere>();

        [Encaust]
        public List<Sphere> Spheres
        {
            get => _spheres;
            set => _spheres = value;
        }


        [Encaust] public string Identifier => EditableIdentifier;
        [SerializeField] private string EditableIdentifier;

        private OnSphereAddedEvent _onSphereAdded = new OnSphereAddedEvent();
        private OnSphereRemovedEvent _onSphereRemoved = new OnSphereRemovedEvent();

        [DontEncaust]
        public OnSphereAddedEvent OnSphereAdded
        {
            get => _onSphereAdded;
            set => _onSphereAdded = value;
        }

        [DontEncaust]
        public OnSphereRemovedEvent OnSphereRemoved
        {
            get => _onSphereRemoved;
            set => _onSphereRemoved = value;
        }
        [DontEncaust]
        public IHasAspects Container=>FucineRoot.Get(); //in case we ever move it

        public void Awake()
        {
            var w=new Watchman();
            w.Register(this);
        }

        public IEnumerable<Sphere> GetDrawPiles()
        {
            var drawDeckSpheres = _spheres.Where(s => s.GoverningSphereSpec.SphereType == typeof(DrawPile));
            return drawDeckSpheres;
        }

        public Sphere GetDrawPile(string forDeckSpecId)
        {
            return _spheres.SingleOrDefault(s =>
                s.GoverningSphereSpec.ActionId == forDeckSpecId && s.GoverningSphereSpec.SphereType==typeof(DrawPile));
        }

        public Sphere GetForbiddenPile(string forDeckSpecId)
        {
            return _spheres.SingleOrDefault(s =>
                s.GoverningSphereSpec.ActionId == forDeckSpecId && s.GoverningSphereSpec.SphereType == typeof(ForbiddenPile));
        }

        public void RegisterFor(IManifestable manifestable)
        {
            throw new NotImplementedException();
        }

        public Sphere TryCreateSphere(SphereSpec spec)
        {
            var newSphere=Watchman.Get<PrefabFactory>().InstantiateSphere(spec, Container);
            newSphere.transform.SetParent(transform);
            _spheres.Add(newSphere);

            OnSphereAdded.Invoke(newSphere);
            return newSphere;
        }

        public Sphere GetSphereById(string id)
        {
            return _spheres.SingleOrDefault(s => s.Id == id);
        }

        public bool VisibleFor(string state)
        {
            return true;
        }

        public bool RelevantTo(string state, Type sphereType)
        {
            return true;
        }

        public bool RemoveSphere(string id,SphereRetirementType retirementType)
        {
            throw new NotImplementedException();
        }

        public void Evoke()
        {
            throw new NotImplementedException();
        }

        public void Dismiss()
        {
            throw new NotImplementedException();
        }

        public bool CanCreateSphere(SphereSpec spec)
        {
            return true;
        }

    }
}
