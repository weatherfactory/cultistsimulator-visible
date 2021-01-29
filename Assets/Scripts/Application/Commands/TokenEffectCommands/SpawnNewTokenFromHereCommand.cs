using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Commands.SituationCommands;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Commands
{
  public  class SpawnNewTokenFromHereCommand: IAffectsTokenCommand
  {
      private SituationCreationCommand _situationCreationCommand;
      private Context _context;
      private SpherePath toSpherePath;

      private bool hasExecuted = false;
        public SpawnNewTokenFromHereCommand(SituationCreationCommand situationCreationCommand, SpherePath toSpherePath, Context context)
        {
            _situationCreationCommand = situationCreationCommand;
            _context = context;
        }

        public bool ExecuteOn(Token token)
        {
            if (!hasExecuted)
            {
                TokenLocation newAnchorLocation;

                if (toSpherePath!= SpherePath.Current())
                    newAnchorLocation = new TokenLocation(Vector3.zero, toSpherePath);
                else
                    newAnchorLocation = token.Location;


                var tokenCreationCommand=new TokenCreationCommand(_situationCreationCommand, newAnchorLocation);
                tokenCreationCommand.Execute(_context);
            }
            return true;
        }

        public bool ExecuteOn(ITokenPayload payload)
        {
            return false;
        }
    }
}
