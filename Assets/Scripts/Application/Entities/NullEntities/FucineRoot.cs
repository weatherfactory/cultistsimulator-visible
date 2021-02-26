using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Entities.NullEntities
{
    [IsEncaustableClass(typeof(RootPopulationCommand))]
    public sealed class FucineRoot: IHasFucinePath,IDominion
    {
        static readonly FucineRoot instance=new FucineRoot();

        static FucineRoot()
        {
            //Jon Skeet says do this because
            // "Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit"
        }

        [DontEncaust]
        public string Id => FucinePath.ROOT.ToString();

        private ITokenPayload _payload;

        private readonly List<Sphere> _spheres=new List<Sphere>();
        [Encaust]
        public List<Sphere> Spheres => new List<Sphere>(_spheres);

        [Encaust]
        public Dictionary<string, int> Mutations { get; }=new AspectsDictionary();
        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {
           //
        }

        public AspectsDictionary GetAspects(bool includeSelf)
        {
            return new AspectsDictionary();
        }

        public static FucineRoot Get()
        {
            return instance;
        }


        public FucinePath GetAbsolutePath()
        {
            return FucinePath.Root();
        }
        [DontEncaust]
        public OnSphereAddedEvent OnSphereAdded { get; }
        [DontEncaust]
        public OnSphereRemovedEvent OnSphereRemoved { get; }
        public void RegisterFor(ITokenPayload payload)
        {
        //
        }


        public Sphere CreateSphere(SphereSpec spec)
        {
              var newSphere = Watchman.Get<PrefabFactory>().InstantiateSphere(spec,this);
             Spheres.Add(newSphere);
             return newSphere;
        }

        public Sphere GetSphereById(string id)
        {
            return _spheres.SingleOrDefault(s => s.Id == id);
        }

        public void AddSphere(Sphere sphere)
        {
            if(!_spheres.Contains(sphere))
                _spheres.Add(sphere);
        }

        public void RemoveSphere(Sphere sphere)
        {
            if (_spheres.Contains(sphere))
                _spheres.Remove(sphere);
        }

    }
}
