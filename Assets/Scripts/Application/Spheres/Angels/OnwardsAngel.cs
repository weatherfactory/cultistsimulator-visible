using System.Collections;
using System.Collections.Generic;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Angels
{
    /// <summary>
    /// Allows us to cue up an itinerary for a token that this angel finds in its sphere
    /// </summary>

    public class OnwardsAngel : IAngel

    {
        protected Token _tokenToSendOnwards;
        protected TokenItinerary _onwardsItinerary;
        private readonly float _watchDuration;
        private float _durationElapsed = 0f;
        private const float DEFAULT_WATCH_DURATION = 40f;
        protected Sphere SphereToWatchOver;

        public int Authority { get; }

        public OnwardsAngel(Token token, TokenItinerary itinerary, Sphere sphereToWatch, float watchDuration)
        {
            _tokenToSendOnwards = token;
            _onwardsItinerary = itinerary;
         SetWatch(sphereToWatch);
            _watchDuration = watchDuration;

        }

        public OnwardsAngel(Token token, TokenItinerary itinerary,Sphere sphereToWatch):this(token,itinerary,sphereToWatch,DEFAULT_WATCH_DURATION)
        {
        }

        public void Act(float seconds, float metaseconds)
        {
            _durationElapsed += seconds;
            if (_durationElapsed > _watchDuration)
                Retire();
            //This is a basic safeguard in case we somehow leave one of these things live indefinitely

            var candidateTokenForSending=SphereToWatchOver.Tokens.Find(t => t == _tokenToSendOnwards);
            if (candidateTokenForSending!=null)
            {
                _onwardsItinerary.Depart(_tokenToSendOnwards, new Context(Context.ActionSource.Unknown));
                Retire();
            }


        }

        public void SetWatch(Sphere sphere)
        {
            SphereToWatchOver = sphere;
        }

        public bool MinisterToDepartingToken(Token token, Context context)
        {
            return false;
        }

        public bool MinisterToEvictedToken(Token token, Context context)
        {
            return false;
        }

        public void Retire()
        {
            Defunct = true;

        }

        public bool Defunct { get; private set; }
        public void ShowRelevantVisibleCharacteristic(List<VisibleCharacteristic> visibleCharacteristics)
        {
  //
        }

        public void HideRelevantVisibleCharacteristic(List<VisibleCharacteristic> visibleCharacteristics)
        {
        //
        }
    }
}