using System;
using System.Collections.Generic;
using System.Linq;

using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;

using UnityEngine;

namespace SecretHistories.UI
{
    [IsEncaustableClass(typeof(PopulateDominionCommand))]
    public abstract class AbstractDominion: MonoBehaviour, IEncaustable
    //A dominion is glue between an IHasAspects object and any spheres it contains.
    //It's used to handle registration and management of contained spheres, and to show/hide a group of spheres.
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
        protected IManifestable _manifestable;

        public Sphere spherePrefab;
        [SerializeField] public bool AcceptsNoteCommands;

        public virtual Sphere CreateAutonamedNativeSphere()
        {

            Type sphereType;
            if(spherePrefab==null)
                sphereType=typeof(ThresholdSphere);
            else
                sphereType=spherePrefab.GetType();
            int sphereCount = _spheres.Count;
            int newSphereIndex = sphereCount;
            string newSphereId = $"{sphereType.Name}_{newSphereIndex}";


            var nativeSphereSpec=new SphereSpec(sphereType,newSphereId);

            return TryCreateOrRetrieveSphere(nativeSphereSpec);
        }

        public abstract Sphere TryCreateOrRetrieveSphere(SphereSpec spec);
        public abstract bool VisibleFor(string state);
        public abstract bool RelevantTo(string state, Type sphereType);
        public abstract bool RemoveSphere(string id,SphereRetirementType retirementType);
        public virtual bool CanCreateSphere(SphereSpec spec)
        {
            if (GetSphereById(spec.Id) != null)
                return false; //no spheres with duplicate id.

            return true;
        }

        [DontEncaust]
        public bool CurrentlyFullyEvoked {
            get
            {
                if (canvasGroupFader == null)
                    return false;
                return canvasGroupFader.IsFullyVisible();
            }}

        [DontEncaust]
        public bool CurrentlyBeingEvoked
        {
            get
            {
                if (canvasGroupFader == null)
                    return false;
                return canvasGroupFader.IsFullyVisible();
            }
        }

        [SerializeField] private CanvasGroupFader canvasGroupFader;

        public virtual void Awake()
        {
            //nothing right now, but a couple of base classes call it
        }

        public virtual void Evoke()
        {
            canvasGroupFader?.Show(); //some subclasses don't need or use it
            
        }

        public virtual void Dismiss()
        {
            canvasGroupFader?.Hide(); //some subclasses don't need or use it
            //foreach (var s in _spheres)
            //    RemoveSphere(s.Id, SphereRetirementType.Graceful);

        }

        public virtual void RegisterFor(IManifestable manifestable)
        {
            _manifestable = manifestable;
            manifestable.RegisterDominion(this);

            //if we have permanent spheres created in editor, as for instance in otherworld prefabs, find and add them here.
            //I decided not to make the references explicit in the editor; worth reconsidering that decision if it causes problems later
            var permanentSpheres = gameObject.GetComponentsInChildren<PermanentSphereSpec>();

            foreach (var permanentSphereSpec in permanentSpheres)
            {
                if (string.IsNullOrEmpty(permanentSphereSpec.ApplyId))
                    NoonUtility.LogWarning("Permanent spec for sphere " + permanentSphereSpec.name + " doesn't have an id specified.");

                var actualSphere = permanentSphereSpec.GetSphereComponent();
                permanentSphereSpec.ApplySpecToSphere(actualSphere);
                _spheres.Add(actualSphere);
            }


            foreach (Sphere s in Spheres)
            {
                manifestable.AttachSphere(s);
                s.SetContainer(manifestable);
            }

        }



        public  Sphere GetSphereById(string Id)
        {
            return Spheres.SingleOrDefault(s => s.Id == Id && !s.Defunct);
        }


     
  }
}
