using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Logic;
using Assets.Scripts.Application.Abstract;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Entities.NullEntities
{
    [IsEncaustableClass(typeof(RootPopulationCommand))]
    public sealed class FucineRoot: IHasAspects,IEncaustable
    {
        private const string IIKEY = "II";
        static FucineRoot _instance=new FucineRoot();

        static FucineRoot()
        {
            //Jon Skeet says do this because
            // "Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit"
            //Trust in the Skeet.
        }

        [DontEncaust]
        public string Id => FucinePath.ROOT.ToString();


        private readonly List<Sphere> _spheres=new List<Sphere>();
        private readonly List<DrawPile> _cardPiles=new List<DrawPile>();
        private Dictionary<string, int> _mutations= new Dictionary<string, int>();

        [Encaust]
        public List<Sphere> Spheres => new List<Sphere>(_spheres);

        [Encaust]
        public List<Token> TokensAtArbitraryPaths => new List<Token>(); //currently this doesn't save anything from a live root.
        //it's here to give us the option of arbitrarily inserting tokens at whatever path, as we do with BH legacies.

        [Encaust] public DealersTable DealersTable => Watchman.Get<DealersTable>();

        [Encaust]
        public bool IsOpen => true; //will this be always true? possibly not if we have an otherworld blocking it?

        [Encaust]
        public Dictionary<string, int> Mutations
        {
            get => _mutations;
            set => _mutations = value;
        }

        [DontEncaust]
        public Token Token
        {
            get
            {
                return NullToken.Create();
            }
        }

        public void SetMutation(string aspectId, int value, bool additive)
        {
            if (_mutations.ContainsKey(aspectId))
            {
                if (additive)
                    _mutations[aspectId] += value;
                else
                    _mutations[aspectId] = value;

                if (_mutations[aspectId] == 0)
                    _mutations.Remove(aspectId);
            }
            else if (value != 0)
            {
                _mutations.Add(aspectId, value);
            }
        }

        public string GetSignature()
        {
            return FucinePath.ROOT.ToString();
        }
        public Sphere GetEnRouteSphere()
        {
            var defaultSphere=Watchman.Get<HornedAxe>().GetDefaultSphere(OccupiesSpaceAs.Unknown);
            return Watchman.Get<HornedAxe>().GetSphereByAbsolutePath(defaultSphere.GoverningSphereSpec.EnRouteSpherePath);
        }

        
        public Sphere GetWindowsSphere()
        {
            var defaultSphere = Watchman.Get<HornedAxe>().GetDefaultSphere(OccupiesSpaceAs.Unknown);
            return Watchman.Get<HornedAxe>().GetSphereByAbsolutePath(defaultSphere.GoverningSphereSpec.WindowsSpherePath);
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

        public FucinePath GetWildPath()
        {
            return FucinePath.Wild();
        }

        public RectTransform GetRectTransform()
        {
            NoonUtility.LogWarning("Trying to get fucine root recttransform; supplying the default sphere rect transfomr.");
            return Watchman.Get<HornedAxe>().GetDefaultSphere(OccupiesSpaceAs.Unknown).GetRectTransform(); //this is most likely what we're expecting
        }


        public Sphere GetSphereById(string id)
        {
            try
            {
                return _spheres.SingleOrDefault(s => s.Id == id);
            }
            catch (Exception e)
            {
                if(_spheres!=null && _spheres.Count>0)
                {
                    string message =
                        $"trying to retrieve sphere with id {id}, but there are {_spheres.Count(s => s.Id == id)} spheres with that id.";
                    NoonUtility.Log(message);
                }
                NoonUtility.LogException(e);

            }

            return null;
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

        

        public int IncrementedIdentity()
        {
            int iiMutationValue;

            if (_mutations.ContainsKey(IIKEY))
                iiMutationValue = (_mutations[IIKEY]);
            else
                iiMutationValue = 0;

            SetMutation(IIKEY,iiMutationValue+1,false);
            return iiMutationValue;
        }


    }
}
