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
using SecretHistories.Spheres;
using UnityEditorInternal;
using UnityEngine;

namespace SecretHistories.UI
{
    [IsEncaustableClass(typeof(PopulateDominionCommand))]
    public abstract class AbstractDominion: MonoBehaviour, IEncaustable

  {
      [Encaust]
        public string Identifier { get; protected set; }

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

        [Encaust]
        public List<Sphere> Spheres=>new List<Sphere>(_spheres);

        protected readonly List<Sphere> _spheres = new List<Sphere>();
        protected OnSphereAddedEvent _onSphereAdded = new OnSphereAddedEvent();
        protected OnSphereRemovedEvent _onSphereRemoved = new OnSphereRemovedEvent();
      

        
        public abstract void RegisterFor(IManifestable manifestable);
       
        public abstract Sphere TryCreateSphere(SphereSpec spec);
        public abstract Sphere GetSphereById(string id);
        public abstract bool VisibleFor(string state);
        public abstract bool RelevantTo(string state, Type sphereType);
        public abstract bool RemoveSphere(string id,SphereRetirementType retirementType);
        public abstract void Evoke();
        public abstract void Dismiss();
        public abstract bool CanCreateSphere(SphereSpec spec);


    }
}
