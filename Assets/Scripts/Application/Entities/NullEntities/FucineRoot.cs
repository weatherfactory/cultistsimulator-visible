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
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Entities.NullEntities
{
    [IsEncaustableClass(typeof(RootPopulationCommand))]
    public sealed class FucineRoot: IHasAspects,IEncaustable
    {
        static FucineRoot _instance=new FucineRoot();

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

        [DontEncaust]
        public Token Token
        {
            get
            {
                return NullToken.Create();
            }
        }

        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {
           //
        }

        public string GetSignature()
        {
            return FucinePath.ROOT.ToString();
        }



        public AspectsDictionary GetAspects(bool includeSelf)
        {
            return new AspectsDictionary();
        }

        public static FucineRoot Get()
        {
            return _instance;
        }

        public static void Reset()
        {
            _instance=new FucineRoot();
        }

        public FucinePath GetAbsolutePath()
        {
            return FucinePath.Root();
        }


        public Sphere GetSphereById(string id)
        {
            return _spheres.SingleOrDefault(s => s.Id == id);
        }

        public void AttachSphere(Sphere sphere)
        {
            if(!_spheres.Contains(sphere))
            {
                _spheres.Add(sphere);
                sphere.SetContainer(this);
            }
        }

        public void DetachSphere(Sphere c)
        {
            _spheres.Remove(c);
        }



    }
}
