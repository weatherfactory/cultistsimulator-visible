using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Meta;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Commands;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres;
using Steamworks;
using UnityEditor;
using UnityEngine;

namespace SecretHistories.UI
{
    /// <summary>
    /// The God of the North watches over travellers. He tracks all current itineraries and can be aware of problems of overlap.
    /// </summary>
    [IsEncaustableClass(typeof(PopulateXamanekCommand))]
    public class Xamanek : MonoBehaviour, IEncaustable
    {

        [Encaust]
        public Dictionary<string, TokenItinerary> CurrentItineraries =>
            new Dictionary<string, TokenItinerary>(_itineraries);

        [Encaust] public HashSet<SphereBlock> CurrentSphereBlocks => new HashSet<SphereBlock>(_sphereBlocks);

        //Keys are token payloads ids. So here we assume Token payload ids are unique. This should be the case.
        //We also assume each token can only ever have one itinerary. We may later regret this but I think it's a wise stricture.
        private readonly Dictionary<string, TokenItinerary> _itineraries = new Dictionary<string, TokenItinerary>();

        private readonly HashSet<SphereBlock> _sphereBlocks=new HashSet<SphereBlock>();

        
        [SerializeField] private GameObject ItinerariesDisplayHolder;
        [SerializeField] private GameObject SphereBlocksDisplayHolder; //not yet implemented
        [SerializeField] private bool MetapauseWhenItineraryStarted;

        public void Awake()
        {
            var r = new Watchman();
            r.Register(this);
            ClearItineraryDisplays();

        }

        public void UpdateItineraryDisplays()
        {
            ClearItineraryDisplays();

            foreach (var i in _itineraries.Where(i => i.Value.IsActive()))
            {
                var newItineraryDisplay = Watchman.Get<PrefabFactory>()
                    .CreateLocally<ItineraryDisplay>(ItinerariesDisplayHolder.transform);
                newItineraryDisplay.DisplayItinerary(i.Key, i.Value);
            }
        }

        private void ClearItineraryDisplays()
        {
            var existingDisplays = ItinerariesDisplayHolder.GetComponentsInChildren<ItineraryDisplay>();
            foreach (var ed in existingDisplays)
                Destroy(ed.gameObject);
        }

        private void DestroyTravelAnimationForToken(Token token)
        {
            var travelAnimation = token.gameObject.GetComponent<TokenTravelAnimation>();
            travelAnimation.Retire();
        }

        public void ItineraryStarted(string tokenPayloadId, TokenTravelItinerary itinerary)
        {
            if (MetapauseWhenItineraryStarted)
            {
                Watchman.Get<Heart>().Metapause();

            }

            if (_itineraries.ContainsKey(tokenPayloadId))
                _itineraries[tokenPayloadId] = itinerary;
            else
                _itineraries.Add(tokenPayloadId, itinerary);
            UpdateItineraryDisplays();
        }

        public void TokenItineraryCompleted(Token token)
        {
            _itineraries.Remove(token.PayloadId);
            DestroyTravelAnimationForToken(token);
            UpdateItineraryDisplays();


        }

        public void TokenItineraryInterrupted(Token token)
        {
            _itineraries.Remove(token.PayloadId);
            DestroyTravelAnimationForToken(token);
            UpdateItineraryDisplays();

        }

        public void RegisterSphereBlock(SphereBlock newBlock)
        {
            _sphereBlocks.Add(newBlock);
        }
        //Note: we *don't* remove sphereblocks routinely when a sphere retires, because the blocks may still apply to a successor in the same location

        public IEnumerable<SphereBlock> GetBlocksForSphereAtPath(FucinePath atPath)
        {
            return _sphereBlocks.Where(sb => sb.AtSpherePath == atPath);
        }

        public int RemoveMatchingBlocks(FucinePath atPath, BlockDirection blockDirection, BlockReason blockReason)
        {
            if (blockDirection == BlockDirection.All)
                return CurrentSphereBlocks.RemoveWhere(cb => cb.AtSpherePath==atPath && cb.BlockReason == blockReason);
            else
                return CurrentSphereBlocks.RemoveWhere(cb => cb.AtSpherePath == atPath &&
                                                             cb.BlockDirection == blockDirection && cb.BlockReason == blockReason);

        }

        public Dictionary<string,TokenItinerary> CurrentItinerariesForPath(FucinePath forPath)
        {
            var matchingItineraries=new Dictionary<string, TokenItinerary>( _itineraries.Where(i =>
                i.Value.DestinationSpherePath == forPath));

            return matchingItineraries;
        }

 
        //public void OnGUI()
        //{
        //    float xStart = 0f;
        //    float yStart = 0f;
        //    foreach(var i in currentTokenTravelItineraries)
        //    {
        //        yStart = yStart + 10;
        //        GUI.Label(new Rect(xStart, yStart, 800, 20), $"{i.TokenName} moving to {i.DestinationSpherePath.ToString()} {i.Anchored3DEndPosition}");
        //    }
        //}


    }



}
