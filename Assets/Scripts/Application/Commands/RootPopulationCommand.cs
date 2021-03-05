﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Constants;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Commands
{
    public class RootPopulationCommand: IEncaustment
    {
        public Dictionary<string, int> Mutations { get; set; }
        public List<SphereCreationCommand> Spheres { get; set; }
        public PopulateDominionCommand DealersTable { get; set; }

        public RootPopulationCommand()
        {
            Mutations=new Dictionary<string, int>();
            Spheres=new List<SphereCreationCommand>();
        }

        public void Execute(Context context)
        {
            var root = FucineRoot.Get();
            foreach(var m in Mutations)
                root.SetMutation(m.Key,m.Value,false);

            foreach(var s in Spheres)
                s.ExecuteOn(root, context);

            DealersTable.Execute(root.DealersTable);
        }

        
        public static RootPopulationCommand RootCommandForLegacy(Legacy startingLegacy)
        {
            var rootCommand=new  RootPopulationCommand();
            var tabletoppath=new FucinePath(Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultWorldSpherePath);
            var tabletopId = tabletoppath.GetEndingPathPart().GetId();
            var tabletopSphereSpec = new SphereSpec(typeof(TabletopSphere), tabletopId);
            var tabletopSphereCreationCommand = new SphereCreationCommand(tabletopSphereSpec);
            tabletopSphereCreationCommand.Tokens.AddRange(startingLegacy.GetTokenCreationCommandsToEnactLegacy());
            rootCommand.Spheres.Add(tabletopSphereCreationCommand);

            rootCommand.DealersTable=new PopulateDominionCommand();

            var allDeckSpecs=Watchman.Get<Compendium>().GetEntitiesAsList<DeckSpec>();
            foreach (var d in allDeckSpecs)
            {
                if (string.IsNullOrEmpty(d.ForLegacy) || startingLegacy.Id == d.ForLegacy)
                {
                    var drawSphereSpec = new SphereSpec(typeof(CardPile), $"{d.Id}_draw");
                    drawSphereSpec.ActionId = d.Id;

                    var drawSphereCommand = new SphereCreationCommand(drawSphereSpec);
                    rootCommand.DealersTable.Spheres.Add(drawSphereCommand);

                    var discardSphereSpec = new SphereSpec(typeof(CardPile), $"{d.Id}_discard");
                    discardSphereSpec.ActionId = d.Id;

                    var discardSphereCommand=new SphereCreationCommand(discardSphereSpec);
                    rootCommand.DealersTable.Spheres.Add(discardSphereCommand);
                }

            }

            
            return rootCommand;
        }
    }
}
