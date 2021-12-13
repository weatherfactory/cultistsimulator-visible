using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Infrastructure
{
    public class SimpleSetup: MonoBehaviour
    {
        public void Start()
        {
            var bookcaseTokenCreationCommand = new TokenCreationCommand();

            
                var bookcaseElementCommand = new ElementStackCreationCommand("bookcase",1);
                var startingDestinationForToken = TokenLocation.Default(Watchman.Get<HornedAxe>().GetDefaultSpherePath());
                TokenCreationCommand tokenCommand =
                    new TokenCreationCommand(bookcaseElementCommand, startingDestinationForToken);
                tokenCommand.Execute(Context.Unknown(), Watchman.Get<HornedAxe>().GetDefaultSphere());
        }
        
    }
}
