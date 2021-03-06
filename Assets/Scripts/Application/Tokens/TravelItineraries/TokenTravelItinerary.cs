using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Tokens;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Entities;
using SecretHistories.States.TokenStates;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace SecretHistories.UI
{

    public class TokenTravelItinerary: TokenItinerary
    {
        
            public override float Elapsed
        {
            //primary candidate for refactoring in itinerary world!
            //let's sort out what keeps a reference to what.
//if we don't change it, we need to make sure there's a JSON-specific constructor to rehydrate Duration and Elapsed

            get
            {
                if(_token==null) return 0;
              var currentTravelAnimation = _token.gameObject.GetComponent<TokenTravelAnimation>();
                if (currentTravelAnimation == null)
                    return 0;

                return currentTravelAnimation.GetDurationElapsed();

            }
        }

        [SerializeField] public float StartScale { get; set; }
        public float EndScale { get; set; }
        //startscale   = 1f, float endScale = 1f)
        private const float DefaultStartScale = 1f;
        private const float DefaultEndScale = 1f;
        private Token _token;
        
        public TokenTravelItinerary()
        {}

        public TokenTravelItinerary(TokenLocation startLocation, TokenLocation endLocation)
        {
            Anchored3DStartPosition = startLocation.Anchored3DPosition;
            Anchored3DEndPosition = endLocation.Anchored3DPosition;
            DestinationSpherePath = endLocation.AtSpherePath;
            StartScale = DefaultStartScale;
            EndScale = DefaultEndScale;
        }

        public TokenTravelItinerary(Vector3 anchored3DStartPosition, Vector3 anchored3DEndPosition)
        {
            //the most basic itinerary: don't change sphere, move in current sphere from point to point,s et default duration based on distance, keep current scale
            Anchored3DStartPosition = anchored3DStartPosition;
            Anchored3DEndPosition = anchored3DEndPosition;

            
            DestinationSpherePath = Watchman.Get<HornedAxe>().GetDefaultSpherePath();
            StartScale = DefaultStartScale;
            EndScale = DefaultEndScale;
        }
        public override string GetDescription()
        {
            return "->";
        }
        /// <summary>
        /// use when a token is already en route, ie has a running animation
        /// </summary>
        /// <param name="token"></param>
        /// <param name="argsContext"></param>
        public void Divert(Token token, Context context)
        {
            var currentTravelAnimation = token.gameObject.GetComponent<TokenTravelAnimation>();
            if (currentTravelAnimation != null)
                currentTravelAnimation.Retire();
            Watchman.Get<Xamanek>().TokenItineraryInterrupted(token);

            Depart(token,context);
        }

        public override void Depart(Token tokenToSend, Context context)
        {

            Sphere destinationSphere= Watchman.Get<HornedAxe>().GetSphereByPath(tokenToSend.Sphere, DestinationSpherePath);


            //I wrote this to cater for save files with unexpectedly invalid destinationspheres as a belt-and-braces approach.
            //It makes the cycle more robust, though, and even though the invalid destinationsphere issue is now fixed, I feel happier leaving it in. WITH LOGGING THOUGH.
            if (destinationSphere == null || !destinationSphere.IsValid)
            {
                context.Description += $"Aborting token departure for {tokenToSend.PayloadId}: couldn't find a valid sphere at {DestinationSpherePath} ";
                //an error's occurred, perhaps because of a save rehydration issue. This makes the itinerary unsalvageable.
                AbortDeparture(tokenToSend, context);
                return;
            }


            _token = tokenToSend;


            Watchman.Get<Xamanek>().ItineraryStarted(tokenToSend.PayloadId, this);

            tokenToSend.Unshroud(true);
            tokenToSend.CurrentState = new TravellingState();

            //create TokenTravelAnimation component
            //set the Arrival event on TokenTravelAnimation so we know how to deal with it @end
            //set the destination & scaling
            //finally, TokenTravelAnimation.Begin starts the journey
            //from then on, it's in the hands of TokenTravelAnimation.ExecuteHeartbeat


            var tokenTravelAnimation = SetUpTokenTravelAnimation(tokenToSend);
  

            SetupBlocksForDestinationSphere(destinationSphere, tokenTravelAnimation);

            //We convert to world positions before sending, because we'll be animating through an EnRouteSphere to a DestinationSphere,
            //and the local positions in those are unlikely to match.
            Vector3 startPositioninWorldSpace = tokenToSend.Sphere.TransformPositionInSphereToWorldSpace(Anchored3DStartPosition);
            Vector3 endPositionInWorldSpace = destinationSphere.TransformPositionInSphereToWorldSpace(Anchored3DEndPosition);

            //is it if scaling ends before travel duration?
            //set a default duration if we don't have a valid one
            if (Duration <= 0)
                SetDefaultDuration();


            tokenTravelAnimation.SetPositions(startPositioninWorldSpace, endPositionInWorldSpace);
            tokenTravelAnimation.SetScaling(StartScale, EndScale, Duration); //1f was the originally set default. I'm not clear atm about the difference between Duration and ScaleDuration 
            
            var enrouteSphere = tokenToSend.Payload.GetEnRouteSphere();
            enrouteSphere.AcceptToken(tokenToSend, context);
            
            destinationSphere.Subscribe(tokenTravelAnimation);

            tokenTravelAnimation.Begin(tokenToSend, context, Duration);

        }

        private void SetDefaultDuration()
        {
            float distance = Vector3.Distance(Anchored3DStartPosition, Anchored3DEndPosition);
            float defaultTravelDuration = Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultTravelDuration;
            Duration = Mathf.Max(defaultTravelDuration, distance * 0.001f);
        }

        private static void SetupBlocksForDestinationSphere(Sphere destinationSphere, TokenTravelAnimation tokenTravelAnimation)
        {
            if (destinationSphere.SphereCategory == SphereCategory.Threshold) //hacky. Something more like a 'max tokens #' would make sense.
            {
                tokenTravelAnimation.OnBlockRedundant += destinationSphere.RemoveMatchingBlocks;
            }
        }

        private TokenTravelAnimation SetUpTokenTravelAnimation(Token tokenToSend)
        {
            var tokenAnimation = tokenToSend.gameObject.AddComponent<TokenTravelAnimation>();
            tokenAnimation.OnTokenArrival += Arrive;
            return tokenAnimation;
        }

        public override void Arrive(Token token,Context context)
        {
            try
            {
                Sphere destinationSphere = Watchman.Get<HornedAxe>().GetSphereByPath(token.Sphere, DestinationSpherePath);

                if (destinationSphere==null || destinationSphere.Equals(null) || !destinationSphere.IsValid || destinationSphere.Defunct)
                    TravelFailed(token);
                else
                {
                    Watchman.Get<Xamanek>().TokenItineraryCompleted(token);
                    token.OnCompletedTravelItinerary();

                    if (TryMergeWithTokenAtDestination(token, destinationSphere))
                        return;
                    else
                        destinationSphere.TryAcceptToken(token,context);
                    
                }

            }
            catch(Exception e)
            {
                NoonUtility.LogException(e); 
                TravelFailed(token);
            }
        }

        private bool TryMergeWithTokenAtDestination(Token incomingToken, Sphere destinationSphere)
        {
            var incomingTokenRect = incomingToken.GetRectInOtherSphere(destinationSphere);
           //If the token has travelled somewhere, it may be going back to its origin stack, and if it's not going back to its origin stack, we probably still want to tidy it up.
           var candidateMergeTokens = destinationSphere.Tokens.Where(t => t.CanMergeWithToken(incomingToken));
           foreach (var cmt in candidateMergeTokens)
           {
               var cmtRect = cmt.GetRectInCurrentSphere();
               if (cmtRect.Overlaps(incomingTokenRect)) //this is very forgiving - not perfect match, just any overlap at all. I think this is preferable
               {
                   if(cmt.CanMergeWithToken(incomingToken)) //we just checked, but it's possible something changed in the background
                       cmt.Payload.InteractWithIncoming(incomingToken);
                   return true;
               }

           }

           return false;
        }

        public override IGhost GetGhost()
        {
            return _token.GetCurrentGhost();
        }

        public override bool IsActive()
        {
            return true;
        }

        private void AbortDeparture(Token token,Context context)
        {
            NoonUtility.LogWarning($"Aborted tokentravelitinerarydeparture for payloadid {token.PayloadId}: {context.Description}");
        }

        private void TravelFailed(Token token)
        {
            
            Watchman.Get<Xamanek>().TokenItineraryCompleted(token);
            token.CurrentState=new TravellingState();
            token.GoAway(new Context(Context.ActionSource.TravelFailed));

        }

     

        /// <summary>
        /// if we want to move to another sphere, set the sphere to travel through and the sphere to end in
        /// </summary>
        /// <param name="enRouteSphere"></param>
        /// <param name="destinationSphere"></param>
        public TokenTravelItinerary WithDestinationSpherePath( FucinePath destinationSpherePath)
        {
            DestinationSpherePath = destinationSpherePath;
            return this;
        }

        public TokenTravelItinerary WithDuration(float duration)
        {
            Duration = duration;
            return this;
        }

        public TokenTravelItinerary WithScaling(float startScale, float endScale)
        {
            StartScale = startScale;
            EndScale = endScale;
            return this;
        }



    }
}