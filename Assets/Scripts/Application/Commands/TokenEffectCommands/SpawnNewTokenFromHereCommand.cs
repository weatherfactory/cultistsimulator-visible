﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Commands
{
  public  class SpawnNewTokenFromThisOneCommand: IAffectsTokenCommand
  {
      private readonly ITokenPayloadCreationCommand _payloadCreationCommand;
      private Context _context;
      private FucinePath _toSpherePath;

      private bool hasExecuted = false;
        public SpawnNewTokenFromThisOneCommand(ITokenPayloadCreationCommand payloadCreationCommand,FucinePath toSpherePath,Context context)
        {
            _payloadCreationCommand = payloadCreationCommand;
            _toSpherePath = toSpherePath;
            _context = context;
        }

        public bool ExecuteOn(Token token)
        {
            if (!hasExecuted)
            {
                TokenLocation newAnchorLocation;

                if (_toSpherePath!= FucinePath.Current())
                    newAnchorLocation = new TokenLocation(Vector3.zero, _toSpherePath);
                else
                    newAnchorLocation = token.Location;
                
                var tokenCreationCommand=new TokenCreationCommand(_payloadCreationCommand, newAnchorLocation).WithSourceToken(token);

                var newToken=tokenCreationCommand.Execute(_context,token.Sphere);
                newToken.HideGhost();
                SoundManager.PlaySfx("SituationTokenSpawn");

                //Sometimes that command will result in a null token - for example if we're trying to spawn a unique situation
                //that already exists. In this case, return false, because nothing's been created.
                if (newToken.IsValid())
                {
                    newToken.Sphere.EvictToken(newToken, _context);
                    return true;
                }
                else
                {
                    //null token, or something else has gone strange
                    newToken.Retire(RetirementVFX.None);
                    return false;
                }
                
            }
            return false;
        }

        public bool ExecuteOn(ITokenPayload payload)
        {
            return false;
        }
    }
}
