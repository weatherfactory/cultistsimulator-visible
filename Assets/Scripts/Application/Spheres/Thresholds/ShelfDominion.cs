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
        public string Id { get; set; }


        [Encaust]
        public List<Sphere> Spheres =>new List<Sphere>(_spheres);

        private OnSphereAddedEvent _onSphereAdded = new OnSphereAddedEvent();
        private OnSphereRemovedEvent _onSphereRemoved = new OnSphereRemovedEvent();

        public string Identifier => SituationDominionEnum.Unknown.ToString();

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
               _spheres.Add(TryCreateSphere(shelfSpaceSpec));
            }
        }
        
        public Sphere TryCreateSphere(SphereSpec spec)
        {
            if (!CanCreateSphere(spec))
                return NullSphere.Create();

            var newSphere=Watchman.Get<PrefabFactory>().InstantiateSphere(spec);
            newSphere.gameObject.transform.SetParent(this.gameObject.transform); //MANUALLY? really? the problem is that the gameobject hierarchy is now quite different from the FucinePath one
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
    }
}
