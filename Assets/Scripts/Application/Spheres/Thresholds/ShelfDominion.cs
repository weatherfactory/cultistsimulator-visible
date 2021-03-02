﻿using System;
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
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Thresholds
{
    [IsEncaustableClass(typeof(PopulateDominionCommand))]
    public class ShelfDominion: MonoBehaviour,IDominion
    {
        private IManifestable _manifestable;
        

        private HashSet<Sphere>_spheres=new HashSet<Sphere>();

        [Encaust]
        public List<Sphere> Spheres =>new List<Sphere>(_spheres);

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


        public void RegisterFor(IManifestable manifestable)
        {
            _manifestable = manifestable;
            manifestable.RegisterDominion(this);

            for (int i = 0; i < 3; i++)
            {
                var shelfSpaceSpec=new SphereSpec(typeof(ShelfSpaceSphere),"shelf" + i);
               _spheres.Add(CreateSphere(shelfSpaceSpec));
            }
        }
        
        public Sphere CreateSphere(SphereSpec spec)
        {
            var newSphere=Watchman.Get<PrefabFactory>().InstantiateSphere(spec);
            newSphere.gameObject.transform.SetParent(this.gameObject.transform); //MANUALLY? really? the problem is that the gameobject hierarchy is now quite different from the FucinePath one
            OnSphereAdded.Invoke(newSphere);
            return newSphere;
        }

        public Sphere GetSphereById(string id)
        {
            return _spheres.SingleOrDefault(s => s.Id == id);
        }
    }
}