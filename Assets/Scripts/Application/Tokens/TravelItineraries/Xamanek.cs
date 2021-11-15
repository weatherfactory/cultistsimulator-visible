using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Fucine;
using SecretHistories.Services;
using UnityEditor;
using UnityEngine;

namespace SecretHistories.UI
{
    /// <summary>
    /// The God of the North watches over travellers. He tracks all current itineraries and can be aware of problems of overlap.
    /// </summary>
    public class Xamanek: MonoBehaviour
    {
        //we assume Token payload ids are unique. This should be the case.
        //we assume each token can only ever have one itinerary. We may later regret this but I think it's a wise stricture.
        private readonly Dictionary<string, TokenItinerary> _currentItineraries = new Dictionary<string, TokenItinerary>();

        [SerializeField] private GameObject ItinerariesDisplayHolder;
        [SerializeField] private bool  MetapauseWhenItineraryStarted;
        public void Awake()
        {
            var r = new Watchman();
            r.Register(this);
            ClearItineraryDisplays();

        }

        public void UpdateItineraryDisplays()
        {
            ClearItineraryDisplays();

            foreach (var i in _currentItineraries.Where(i=>i.Value.IsActive()))
            {
                var newItineraryDisplay = Watchman.Get<PrefabFactory>()
                    .CreateLocally<ItineraryDisplay>(ItinerariesDisplayHolder.transform);
                newItineraryDisplay.DisplayItinerary(i.Key,i.Value);
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

        public void ItineraryStarted(string tokenPayloadId,TokenTravelItinerary itinerary)
        {
            if (MetapauseWhenItineraryStarted)
            {
                Watchman.Get<Heart>().Metapause();
              //  Watchman.Get<Mi>()
            }
            _currentItineraries.Add(tokenPayloadId,itinerary);
            UpdateItineraryDisplays();
        }

        public void TokenItineraryCompleted(Token token)
        {
            _currentItineraries.Remove(token.PayloadId);
            DestroyTravelAnimationForToken(token);
            UpdateItineraryDisplays();


        }

        public void TokenItineraryInterrupted(Token token)
        {
            _currentItineraries.Remove(token.PayloadId);
            DestroyTravelAnimationForToken(token);
            UpdateItineraryDisplays();

        }


        public Dictionary<string,TokenItinerary> CurrentItinerariesForPath(FucinePath forPath)
        {
            var matchingItineraries=new Dictionary<string, TokenItinerary>( _currentItineraries.Where(i =>
                i.Value.DestinationSpherePath == forPath));

            return matchingItineraries;
        }

        //public bool ItineraryWouldClash(TokenTravelItinerary checkItinerary)
        //{
        //    if (currentTokenTravelItineraries.Exists(i =>
        //        i.DestinationSpherePath == checkItinerary.DestinationSpherePath &&
        //        i.Anchored3DEndPosition == checkItinerary.Anchored3DEndPosition))
        //        return true;
        //    return false;
        //}

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
