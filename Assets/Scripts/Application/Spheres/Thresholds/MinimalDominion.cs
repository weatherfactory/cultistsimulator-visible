using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Services;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.UI
{
    [IsEncaustableClass(typeof(PopulateDominionCommand))]
    public class MinimalDominion: MonoBehaviour, IDominion
    {
        private IManifestable manifestable;
        private readonly List<Sphere> _spheres=new List<Sphere>();

        [Encaust]
        public List<Sphere> Spheres => new List<Sphere>(_spheres);
        [DontEncaust]
        public OnSphereAddedEvent OnSphereAdded { get;  }
        [DontEncaust]
        public OnSphereRemovedEvent OnSphereRemoved { get; }

        public Sphere GetSphereById(string Id)
        {
            return Spheres.SingleOrDefault(s => s.Id == Id);
        }

        public Sphere CreateSphere(SphereSpec spec)
        {
            var newSphere = Watchman.Get<PrefabFactory>().InstantiateSphere(spec, manifestable);
            _spheres.Add(newSphere);
            return newSphere;
        }

        public void RegisterFor(IManifestable manifestable)
        {
            manifestable = manifestable;
            manifestable.RegisterDominion(this);

            foreach (Sphere s in Spheres)
            {
                s.SetContainer(manifestable);
            }

          
        }
    }


}
