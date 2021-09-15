using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using UnityEditor;
using UnityEngine;

namespace SecretHistories.UI
{
    public class Xamanek: MonoBehaviour
    {
        [SerializeField]
  private readonly List<TokenTravelItinerary> currentTokenTravelItineraries=new List<TokenTravelItinerary>();

        public void Awake()
        {
            var r = new Watchman();
            r.Register(this);
        }

        public void ItineraryStarted(TokenTravelItinerary itinerary)
        {
            currentTokenTravelItineraries.Add(itinerary);
        }

        public void ItineraryCompleted(TokenTravelItinerary itinerary)
        {
            currentTokenTravelItineraries.Remove(itinerary);
        }

        public List<TokenTravelItinerary> CurrentItineraries()
        {
            return new List<TokenTravelItinerary>(currentTokenTravelItineraries);
        }

        public List<TokenTravelItinerary> CurrentItinerariesForPath(FucinePath forPath)
        {
            return new List<TokenTravelItinerary>(currentTokenTravelItineraries.Where(i =>
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
