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
    [IsEncaustableClass(typeof(PopulateDominionCommand))]
    public class MinimalDominion: MonoBehaviour, IDominion
    {
        private IManifestable _manifestable;
        private readonly List<Sphere> _spheres=new List<Sphere>();
        
        [Encaust]
        public string Id { get; set; }


        [Encaust]
        public List<Sphere> Spheres => new List<Sphere>(_spheres);

        public DominionEnum Identifier { get; set; }


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

        public MinimalDominion()
        {
            Identifier = DominionEnum.Unknown;
        }

        public Sphere GetSphereById(string Id)
        {
            return Spheres.SingleOrDefault(s => s.Id == Id && !s.Defunct);
        }

        public bool VisibleFor(StateEnum state)
        {
            return true;
        }

        public bool RelevantTo(StateEnum state, Type sphereType)
        {
            return true;

        }

        public bool RemoveSphere(string id,SphereRetirementType retirementType)
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

        public void Evoke()
        {
            //
        }

        public void Dismiss()
        {
         //
        }

        public bool CanCreateSphere(SphereSpec spec)
        {
            return true;
        }

        public Sphere TryCreateSphere(SphereSpec spec)
        {
            if (!CanCreateSphere(spec))
                return NullSphere.Create();

            var newSphere = Watchman.Get<PrefabFactory>().InstantiateSphere(spec, _manifestable);
            _spheres.Add(newSphere);
            return newSphere;
        }

        public void RegisterFor(IManifestable manifestable)
        {
            _manifestable = manifestable;
            manifestable.RegisterDominion(this);

            foreach (Sphere s in Spheres)
            {
                s.SetContainer(manifestable);
            }

          
        }
    }


}
