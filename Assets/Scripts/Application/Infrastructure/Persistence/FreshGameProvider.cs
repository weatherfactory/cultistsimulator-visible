using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Assets.Scripts.Application.Commands;
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
    /// <summary>
    /// ccompletely new game, ab initio
    /// </summary>
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
            _persistedGameState= PersistedGameState.ForLegacy(StartingLegacy);

        }

        

    }
}
