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
            //only use the location sphere is for some reason we don't have a valid sphere to accept the token
            if (sphere == null && Location.AtSpherePath.IsValid())
                sphere = Watchman.Get<HornedAxe>().GetSphereByPath(Location.AtSpherePath);

            var token = Watchman.Get<PrefabFactory>().CreateLocally<Token>(sphere.GetReferenceRectTransform());
            token.TokenRectTransform.anchoredPosition3D = Location.Anchored3DPosition;
            
            sphere.AcceptToken(token, context);

            var payloadForToken = Payload.Execute(context);

            token.SetPayload(payloadForToken);
            
            payloadForToken.FirstHeartbeat();

            
            if (_sourceToken != null)
            {
                SetTokenTravellingFromSpawnPoint(token);
            }

            SoundManager.PlaySfx("SituationTokenCreate");

            return token;
        }

        private void SetTokenTravellingFromSpawnPoint(Token token)
        {
            
            var enrouteSphere = token.Payload.GetEnRouteSphere();

            var spawnedTravelItinerary = new TokenTravelItinerary(_sourceToken.TokenRectTransform.anchoredPosition3D,
                    token.Sphere.Choreographer.GetFreeLocalPosition(token,
                        _sourceToken.ManifestationRectTransform.anchoredPosition))
                .WithDuration(1f)
                .WithDestinationSpherePath(token.Sphere.GetAbsolutePath())
                .WithScaling(0f, 1f);

            token.TravelTo(spawnedTravelItinerary, new Context(Context.ActionSource.SpawningAnchor));
        }
    }
}
