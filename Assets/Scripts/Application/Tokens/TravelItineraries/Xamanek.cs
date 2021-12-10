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
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Events;
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
    public class Xamanek : MonoBehaviour, IEncaustable, ISphereCatalogueEventSubscriber
    {

        [Encaust]
        public Dictionary<string, TokenItinerary> CurrentItineraries =>
            new Dictionary<string, TokenItinerary>(_itineraries);

        [Encaust] public HashSet<SphereBlock> CurrentSphereBlocks => new HashSet<SphereBlock>(_sphereBlocks);

        //Keys are token payloads ids. So here we assume Token payload ids are unique. This should be the case.
        //We also assume each token can only ever have one itinerary. We may later regret this but I think it's a wise stricture.
        private readonly Dictionary<string, TokenItinerary> _itineraries = new Dictionary<string, TokenItinerary>();

//Tracking blocks as independent state might yet turn out to be redundant. We only use it in one obscure place (Ingress) at time of writing
        private readonly HashSet<SphereBlock> _sphereBlocks=new HashSet<SphereBlock>();

        
        [SerializeField] private GameObject ItinerariesDisplayHolder;
        [SerializeField] private GameObject SphereBlocksDisplayHolder; //not yet implemented
        [SerializeField] private bool MetapauseWhenItineraryStarted;

        public void Awake()
        {
            var r = new Watchman();
            r.Register(this);

            ClearItineraryDisplays();
            ClearSphereBlockDisplays();

        }

        public void Start()
        {
            Watchman.Get<HornedAxe>().Subscribe(this);
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


        private void ClearSphereBlockDisplays()
        {
            var sbds = SphereBlocksDisplayHolder.GetComponentsInChildren<SphereBlockDisplay>();
            foreach(var sbd in sbds)
                Destroy(sbd.gameObject);
        }
        public void UpdateSphereBlockDisplays()
        {
            ClearSphereBlockDisplays();

            foreach (var s in _sphereBlocks)
            {
                var newSbDisplay = Watchman.Get<PrefabFactory>()
                    .CreateLocally<SphereBlockDisplay>(SphereBlocksDisplayHolder.transform);
                newSbDisplay.DisplaySphereBlock(s);
            }
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
            UpdateSphereBlockDisplays();
        }
        //Note: we *don't* remove sphereblocks routinely when a sphere retires, because the blocks may still apply to a successor in the same location

        public IEnumerable<SphereBlock> GetBlocksForSphereAtPath(FucinePath atPath)
        {
            var eligible = new List<SphereBlock>();
            foreach(var sb in _sphereBlocks)
                if (sb.AtSpherePath.Conforms(atPath))
                    eligible.Add(sb);
            return eligible;
        }


        public int RemoveMatchingBlocks(FucinePath atPath, BlockDirection blockDirection, BlockReason blockReason)
        {
            int blocksRemoved = 0;
            if (blockDirection == BlockDirection.All)
                blocksRemoved= _sphereBlocks.RemoveWhere(cb => cb.AtSpherePath.Conforms(atPath)  && cb.BlockReason == blockReason);
            else
                blocksRemoved = _sphereBlocks.RemoveWhere(cb => cb.AtSpherePath.Conforms(atPath) &&
                                                                cb.BlockDirection == blockDirection && cb.BlockReason == blockReason);
            UpdateSphereBlockDisplays();

            return blocksRemoved;
        }

        public Dictionary<string,TokenItinerary> GetCurrentItinerariesForPath(FucinePath forPath)
        {
            var matchingItineraries=new Dictionary<string, TokenItinerary>( _itineraries.Where(i =>
                i.Value.DestinationSpherePath.Conforms(forPath)));

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


        public void OnSphereChanged(SphereChangedArgs args)
        {
            
        }

        public void OnTokensChanged(SphereContentsChangedEventArgs args)
        {

        }

        public void OnTokenInteraction(TokenInteractionEventArgs args)
        {
           //
        }
    }



}
