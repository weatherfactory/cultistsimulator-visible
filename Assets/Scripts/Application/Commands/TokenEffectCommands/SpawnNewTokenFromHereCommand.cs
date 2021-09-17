using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Commands
{
  public  class SpawnNewTokenFromThisOneCommand: IAffectsTokenCommand
  {
      private readonly ITokenPayloadCreationCommand _payloadCreationCommand;
      private Context _context;
      private FucinePath toSpherePath;

      private bool hasExecuted = false;
        public SpawnNewTokenFromThisOneCommand(ITokenPayloadCreationCommand payloadCreationCommand,Context context)
        {
            _payloadCreationCommand = payloadCreationCommand;
            _context = context;
        }

        public bool ExecuteOn(Token token)
        {
            if (!hasExecuted)
            {
                TokenLocation newAnchorLocation;

                if (toSpherePath!= FucinePath.Current())
                    newAnchorLocation = new TokenLocation(Vector3.zero, toSpherePath);
                else
                    newAnchorLocation = token.Location;
                
                var tokenCreationCommand=new TokenCreationCommand(_payloadCreationCommand, newAnchorLocation).WithSourceToken(token);

                var newToken=tokenCreationCommand.Execute(_context,token.Sphere);
                newToken.Sphere.EvictToken(newToken,_context);
            }
            return true;
        }

        public bool ExecuteOn(ITokenPayload payload)
        {
            return false;
        }
    }
}
