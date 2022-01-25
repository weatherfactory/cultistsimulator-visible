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
                //very limited!
                var effectPath = linked.ToPath;

                var startupRecipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(linked.Id);
                var startupVerb = Watchman.Get<Compendium>().GetEntityById<Verb>(startupRecipe.ActionId);
                if(startupVerb.Spontaneous)
                {
                    ApplyEffects(startupRecipe, effectPath, commands);
                }
                else
                {
                    var situationCreationCommand = new SituationCreationCommand().WithVerbId(startupVerb.Id).WithRecipeId(startupRecipe.Id);
                    var verbCreationCommand =
                        new TokenCreationCommand(situationCreationCommand, TokenLocation.Default(effectPath));
                    commands.Add(verbCreationCommand);
                }

            }


            return commands;
        }

        private static void ApplyEffects(Recipe startupRecipe, FucinePath effectPath, List<TokenCreationCommand> commands)
        {
            foreach (var effect in startupRecipe.Effects)
            {
                string elementId = effect.Key;
                int quantity =
                    int.Parse(effect
                        .Value); //it won't work with rich effects. Refactor recipe execution out of situations wand we can do this!
                var elementStackCreationCommand = new ElementStackCreationCommand(elementId, quantity);
                TokenCreationCommand startingTokenCreationCommand =
                    new TokenCreationCommand(elementStackCreationCommand, TokenLocation.Default(effectPath));

                commands.Add(startingTokenCreationCommand);
            }
        }
    }
}
