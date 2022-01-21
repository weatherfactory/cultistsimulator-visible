using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Infrastructure
{
    public class BHTokenSetupChamberlain: AbstractTokenSetupChamberlain
    {
        public override List<TokenCreationCommand> GetDefaultSphereTokenCreationCommandsToEnactLegacy(Legacy forLegacy)
        {

            return new List<TokenCreationCommand>();
        }

        public override List<TokenCreationCommand> GetArbitraryPathTokenCreationCommandsToEnactLegacy(Legacy forLegacy)
        {
            //add this to CS as well so people can play with it?
            var commands = new List<TokenCreationCommand>();

            
            foreach (var linked in forLegacy.Startup)
            {
                //very limited! at the moment just grabs the path from the link and the effects from the recipe
                //doesn't check challenges, reqs, warmup, any of that stuff
                var startupRecipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(linked.Id);

                var effectPath = linked.ToPath;

                foreach (var effect in startupRecipe.Effects)
                {
                    string elementId = effect.Key;
                    int quantity = int.Parse(effect.Value); //it won't work with rich effects. Refactor recipe execution out of situations wand we can do this!
                    var elementStackCreationCommand = new ElementStackCreationCommand(effect.Key, quantity);
                    TokenCreationCommand startingTokenCreationCommand = new TokenCreationCommand(elementStackCreationCommand, TokenLocation.Default(effectPath));
                    
                    commands.Add(startingTokenCreationCommand);
                }
            }


            return commands;
        }
    }
}
