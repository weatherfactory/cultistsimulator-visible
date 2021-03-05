using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [Encaust]
        public DominionEnum Identifier => DominionEnum.Unknown;

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

        public void RegisterFor(IManifestable manifestable)
        {
            throw new NotImplementedException();
        }

        public Sphere CreateSphere(SphereSpec spec)
        {
            var newSphere=Watchman.Get<PrefabFactory>().InstantiateSphere(spec, Container);
            newSphere.transform.SetParent(transform);
            _spheres.Add(newSphere);
            
            OnSphereAdded.Invoke(newSphere);
            return newSphere;
        }

        public Sphere GetSphereById(string id)
        {
            throw new NotImplementedException();
        }

        public bool RemoveSphere(string id)
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
    }
}
