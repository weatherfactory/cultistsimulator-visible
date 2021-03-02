using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using Assets.Scripts.Application.Interfaces;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Commands
{
    public class TokenCreationCommand:IEncaustment
    {
        public TokenLocation Location { get; set; }
        public TokenTravelItinerary CurrentItinerary { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public ITokenPayloadCreationCommand Payload { get; set; }
        public bool Defunct { get; set; }
        private Token _sourceToken;

        public TokenCreationCommand()
        {
            Location=TokenLocation.Default(FucinePath.Root()); // we expect the sphere to be overwritten in Execute.The SpherePath bit of Location here is redundant
        }

        public TokenCreationCommand(ITokenPayloadCreationCommand payload,TokenLocation location)
        {
            Payload = payload;
            Location = location; // we expect the sphere to be overwritten in Execute.The SpherePath bit of Location here is redundant
        }

        public TokenCreationCommand(ElementStack elementStack, TokenLocation location)
        {
            var elementStackEncaustery = new Encaustery<ElementStackCreationCommand>();
            Payload = elementStackEncaustery.Encaust(elementStack);
            Location = location;
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

            var token = Watchman.Get<PrefabFactory>().CreateLocally<Token>(sphere.transform);
            
            var payloadForToken = Payload.Execute(context,sphere);

            token.SetPayload(payloadForToken);
            sphere.AcceptToken(token, context);
            payloadForToken.FirstHeartbeat();

            token.transform.localPosition = Location.Anchored3DPosition;

            if (_sourceToken != null)
            {
                SetTokenTravellingFromSpawnPoint(token);
            }

            SoundManager.PlaySfx("SituationTokenCreate");

            return token;
        }

        private void SetTokenTravellingFromSpawnPoint(Token token)
        {
            
            var enrouteSphere = token.Sphere.GetEnRouteSphere();

            var spawnedTravelItinerary = new TokenTravelItinerary(_sourceToken.TokenRectTransform.anchoredPosition3D,
                    token.Sphere.Choreographer.GetFreeLocalPosition(token,
                        _sourceToken.ManifestationRectTransform.anchoredPosition))
                .WithDuration(1f)
                .WithSphereRoute(enrouteSphere, token.Sphere)
                .WithScaling(0f, 1f);

            token.TravelTo(spawnedTravelItinerary, new Context(Context.ActionSource.SpawningAnchor));
        }
    }
}
