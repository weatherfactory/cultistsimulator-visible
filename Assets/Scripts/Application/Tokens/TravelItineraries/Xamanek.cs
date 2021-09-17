using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Fucine;
using UnityEditor;
using UnityEngine;

namespace SecretHistories.UI
{
    /// <summary>
    /// The God of the North watches over travellers. He tracks all current itineraries and can be aware of problems of overlap.
    /// </summary>
    public class Xamanek: MonoBehaviour
    {
        [SerializeField]
  private readonly List<TokenItinerary> currentTokenTravelItineraries=new List<TokenItinerary>();

        public void Awake()
        {
            var r = new Watchman();
            r.Register(this);
        }

        private void DestroyTravelAnimationForToken(Token token)
        {
            var travelAnimation = token.gameObject.GetComponent<TokenTravelAnimation>();
            travelAnimation.Retire();
        }

        public void ItineraryStarted(TokenTravelItinerary itinerary)
        {
            currentTokenTravelItineraries.Add(itinerary);
        }

        public void TokenItineraryCompleted(Token token)
        {
            currentTokenTravelItineraries.Remove(token.CurrentItinerary);
            DestroyTravelAnimationForToken(token);

        }

        public void TokenItineraryInterrupted(Token token)
        {
            currentTokenTravelItineraries.Remove(token.CurrentItinerary);
            DestroyTravelAnimationForToken(token);
  
        }

        
        public List<TokenItinerary> CurrentItineraries()
        {
            return new List<TokenItinerary>(currentTokenTravelItineraries);
        }

        public List<TokenItinerary> CurrentItinerariesForPath(FucinePath forPath)
        {
            return new List<TokenItinerary>(currentTokenTravelItineraries.Where(i =>
                i.DestinationSpherePath == forPath));
        }

        public bool ItineraryWouldClash(TokenTravelItinerary checkItinerary)
        {
            if (currentTokenTravelItineraries.Exists(i =>
                i.DestinationSpherePath == checkItinerary.DestinationSpherePath &&
                i.Anchored3DEndPosition == checkItinerary.Anchored3DEndPosition))
                return true;
            return false;
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
