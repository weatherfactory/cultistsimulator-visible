using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEditorInternal.VR;

namespace SecretHistories.Infrastructure.Persistence

{
    public class FreshGameProvider: GamePersistenceProvider
    {
        public Legacy StartingLegacy { get; set; }


        protected override string GetSaveFileLocation()
        {
            return string.Empty;
        }


        public FreshGameProvider(Legacy startingLegacy)
        {
            StartingLegacy = startingLegacy;
        }

        public override void DepersistGameState()
        {
            
            Sphere tabletopSphere = Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere();
            
            SituationCreationCommand startingSituation = new SituationCreationCommand(StartingLegacy.StartingVerbId, NullRecipe.Create().Id, new SituationPath(StartingLegacy.StartingVerbId), StateEnum.Unstarted);
            TokenCreationCommand startingTokenCommand = new TokenCreationCommand(startingSituation, TokenLocation.Default().WithSphere(tabletopSphere));
            _persistedGameState.TokenCreationCommands.Add(startingTokenCommand);

            AspectsDictionary startingElements = new AspectsDictionary();
            startingElements.CombineAspects(StartingLegacy.Effects);  //note: we don't reset the chosen legacy. We assume it remains the same until someone dies again.

            foreach (var e in startingElements)
            {
                var elementStackCreationCommand = new ElementStackCreationCommand(e.Key, e.Value);
                TokenCreationCommand startingStackCommand = new TokenCreationCommand(elementStackCreationCommand, TokenLocation.Default().WithSphere(tabletopSphere));
                _persistedGameState.TokenCreationCommands.Add(startingStackCommand);
            }

            var cc = CharacterCreationCommand.IncarnateFromLegacy(StartingLegacy);
            _persistedGameState.CharacterCreationCommands.Add(cc);

            var notificationCommand=new AddNoteCommand(StartingLegacy.Label,StartingLegacy.Description,new Context(Context.ActionSource.Loading));

            _persistedGameState.NotificationCommands.Add(notificationCommand);

        }

        

    }
}
