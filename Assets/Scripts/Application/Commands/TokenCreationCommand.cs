using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using Assets.Scripts.Application.Interfaces;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Tokens;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.States;
using SecretHistories.UI;

namespace SecretHistories.Commands
{
    public class TokenCreationCommand:IEncaustment
    {
        public TokenLocation Location { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public TokenItinerary CurrentItinerary { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public ITokenPayloadCreationCommand Payload { get; set; }
        public bool Defunct { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public TokenState CurrentState { get; set; }
        private Token _sourceToken;

        public TokenCreationCommand()
        {
            Location=TokenLocation.Default(FucinePath.Root()); // we expect the sphere to be overwritten in Execute.The SpherePath bit of Location here is redundant
            CurrentItinerary = new TokenInertItinerary();
        }

        public TokenCreationCommand(ITokenPayloadCreationCommand payload,TokenLocation location)
        {
            Payload = payload;
            Location = location; // we expect the sphere to be overwritten in Execute.The SpherePath bit of Location here is redundant
            CurrentItinerary = new TokenInertItinerary();

        }

        public TokenCreationCommand(ElementStack elementStack, TokenLocation location)
        {
            var elementStackEncaustery = new Encaustery<ElementStackCreationCommand>();
            Payload = elementStackEncaustery.Encaust(elementStack);
            Location = location;
            CurrentItinerary = new TokenInertItinerary();

        }

        public TokenCreationCommand WithSourceToken(Token sourceToken)
        {
            _sourceToken = sourceToken;
            return this;
        }

        public TokenCreationCommand WithElementStack(string elementId,int quantity)
        {
            Payload = new ElementStackCreationCommand(elementId, quantity);
            return this;
        }


        public Token Execute(Context context,Sphere sphere)
        {

            var payloadForToken = Payload.Execute(context); //do this first, so we can decide not to instantiate the token if the payload turns out to be invalid (eg, an attempt to create a unique verb twice)
            if(!payloadForToken.IsValid())
                return NullToken.Create();

            var newToken = InstantiateTokenInSphere(context, sphere);

            newToken.SetPayload(payloadForToken);
            payloadForToken.FirstHeartbeat();

            
            if (_sourceToken != null)
            {
                SetTokenTravellingFromSourceToken(newToken,_sourceToken);
            }

            return newToken;
        }

        private Token InstantiateTokenInSphere(Context context, Sphere sphere)
        {
            //only use the location sphere if for some reason we don't have a valid sphere to accept the token
            if (sphere == null && Location.AtSpherePath.IsValid())
                sphere = Watchman.Get<HornedAxe>().GetSphereByPath(Location.AtSpherePath);

            var token = Watchman.Get<PrefabFactory>().CreateLocally<Token>(sphere.GetRectTransform());
            token.TokenRectTransform.anchoredPosition3D = Location.Anchored3DPosition;
            sphere.AcceptToken(token, context);
            return token;
        }

        private void SetTokenTravellingFromSourceToken(Token newToken,Token fromSourceToken)
        {
            
            var enrouteSphere = newToken.Payload.GetEnRouteSphere();

            var spawnedTravelItinerary = new TokenTravelItinerary(fromSourceToken.TokenRectTransform.anchoredPosition3D,
                    newToken.Sphere.Choreographer.GetFreeLocalPosition(newToken,
                        fromSourceToken.ManifestationRectTransform.anchoredPosition))
                .WithDuration(1f)
                .WithDestinationSpherePath(newToken.Sphere.GetAbsolutePath())
                .WithScaling(0f, 1f);

            newToken.TravelTo(spawnedTravelItinerary, new Context(Context.ActionSource.SpawningAnchor));
        }
    }
}
