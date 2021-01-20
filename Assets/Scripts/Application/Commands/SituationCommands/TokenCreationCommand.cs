using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.Services;
using SecretHistories.UI;

namespace Assets.Scripts.Application.Commands.SituationCommands
{
    public class TokenCreationCommand
    {
        private readonly IVerb _forVerb;
        private readonly TokenLocation _location;

        public TokenCreationCommand(IVerb forVerb,TokenLocation location)
        {
            _forVerb = forVerb;
            _location = location;
        }


        public Token Execute(SphereCatalogue sphereCatalogue)
        {
            var sphere = sphereCatalogue.GetSphereByPath(_location.AtSpherePath);
            var token = Watchman.Get<PrefabFactory>().CreateLocally<Token>(sphere.transform);
            token.SetVerb(_forVerb);
    
            sphere.AcceptToken(token, new Context(Context.ActionSource.Unknown));
            token.transform.localPosition = _location.Anchored3DPosition;
            return token;
        }
    }
}
