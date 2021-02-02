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
using SecretHistories.Interfaces;
using SecretHistories.Services;
using SecretHistories.UI;

namespace Assets.Scripts.Application.Commands.SituationCommands
{
    public class TokenCreationCommand:IEncaustment
    {
        public TokenLocation Location { get; set; }
        public TokenTravelItinerary CurrentItinerary { get; set; }
        public ITokenPayloadCreationCommand Payload { get; set; }
        public bool Defunct { get; set; }
        private Token _sourceToken;

        public TokenCreationCommand()
        {

        }

        public TokenCreationCommand(ITokenPayloadCreationCommand payload,TokenLocation location)
        {
            Payload = payload;
            Location = location;
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


        public string ToJson()
        {
            string output= JsonConvert.SerializeObject(this);
            return output;
        }

        public Token Execute(Context context)
        {
            var sphereCatalogue = Watchman.Get<SphereCatalogue>();

            var sphere = sphereCatalogue.GetSphereByPath(Location.AtSpherePath);
            var token = Watchman.Get<PrefabFactory>().CreateLocally<Token>(sphere.transform);
            
            token.SetPayload(Payload.Execute(context));
    
            sphere.AcceptToken(token, new Context(Context.ActionSource.Unknown));
            token.transform.localPosition = Location.Anchored3DPosition;

            if (_sourceToken != null)
            {
                var enRouteSpherePath =
                    new SpherePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWindowSpherePath);

                var enrouteSphere = sphereCatalogue.GetSphereByPath(enRouteSpherePath);
                
                var spawnedTravelItinerary = new TokenTravelItinerary(_sourceToken.TokenRectTransform.anchoredPosition3D,
                        token.Sphere.Choreographer.GetFreeLocalPosition(token, _sourceToken.ManifestationRectTransform.anchoredPosition))
                    .WithDuration(1f)
                    .WithSphereRoute(enrouteSphere, token.Sphere)
                    .WithScaling(0f, 1f);

                token.TravelTo(spawnedTravelItinerary, new Context(Context.ActionSource.SpawningAnchor));
            }

            SoundManager.PlaySfx("SituationTokenCreate");

            return token;
        }
    }
}
