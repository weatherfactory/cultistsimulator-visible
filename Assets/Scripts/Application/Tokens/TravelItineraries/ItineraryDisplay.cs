using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using TMPro;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries
{
    public class ItineraryDisplay: MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI Type;
        [SerializeField]
        private TextMeshProUGUI From;
        [SerializeField]
        private TextMeshProUGUI DurationAndRemaining;
        [SerializeField]
        private TextMeshProUGUI To;

        public void DisplayItinerary(string tokenPayloadId, TokenItinerary itinerary)
        {
            Type.text = itinerary.GetDescription();
            From.text = itinerary.Anchored3DStartPosition.ToString();
            DurationAndRemaining.text = itinerary.Duration.ToString();
            To.text = $"{itinerary.DestinationSpherePath}\n{itinerary.Anchored3DEndPosition}";

        }
    }
}
