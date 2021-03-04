using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public DominionEnum Identifier => DominionEnum.Unknown;

        [DontEncaust]
        public OnSphereAddedEvent OnSphereAdded { get;  }
        [DontEncaust]
        public OnSphereRemovedEvent OnSphereRemoved { get; }

        public Sphere GetSphereById(string Id)
        {
            return Spheres.SingleOrDefault(s => s.Id == Id);
        }

        public bool RemoveSphere(string id)
        {
            var sphereToRemove = GetSphereById(id);
            if (sphereToRemove != null)
            {
                _spheres.Remove(sphereToRemove);
                OnSphereRemoved.Invoke(sphereToRemove);
                sphereToRemove.Retire(SphereRetirementType.Graceful);
                return true;
            }
            else
                return false;
        }

        public Sphere CreateSphere(SphereSpec spec)
        {
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
