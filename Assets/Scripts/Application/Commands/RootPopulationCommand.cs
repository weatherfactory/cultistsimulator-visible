using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Assets.Scripts.Application.Infrastructure;
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

        public List<TokenCreationCommand> TokensAtArbitraryPaths { get; set; }
        public bool IsOpen { get; set; }

        public RootPopulationCommand()
        {
            Mutations=new Dictionary<string, int>();
            Spheres=new List<SphereCreationCommand>();
            TokensAtArbitraryPaths = new List<TokenCreationCommand>();
        }

        

        public void Execute(Context context)
        {
            var root = FucineRoot.Get();
            foreach(var m in Mutations)
                root.SetMutation(m.Key,m.Value,false);

            foreach(var s in Spheres)
                s.ExecuteOn(root, context); 

            foreach (var t in TokensAtArbitraryPaths)
                 t.Execute(context, null);

            DealersTable.Execute(root.DealersTable);
        }

        
        public static SphereCreationCommand DefaultSphereCreationCommand()
        {

            var dictum = Watchman.Get<Compendium>().GetSingleEntity<Dictum>();
            var tabletopPath = new FucinePath(dictum.DefaultWorldSpherePath);

            var tabletopId = tabletopPath.GetEndingPathPart().GetId();

            var defaultWorldSphereType = Type.GetType(dictum.WorldSphereType);

            var tabletopSphereSpec = new SphereSpec(defaultWorldSphereType, tabletopId);
            var tabletopSphereCreationCommand = new SphereCreationCommand(tabletopSphereSpec);
            return tabletopSphereCreationCommand;
        }

        public static RootPopulationCommand RootCommandForLegacy(Legacy startingLegacy)
        {
            var rootCommand=new  RootPopulationCommand();
            var defaultSphereCreationComand = DefaultSphereCreationCommand();

            var chamberlain = startingLegacy.GetTokenSetupChamberlain(Watchman.Get<MetaInfo>().GameId);

            defaultSphereCreationComand.Tokens.AddRange(chamberlain.GetDefaultSphereTokenCreationCommandsToEnactLegacy(startingLegacy));
            rootCommand.Spheres.Add(defaultSphereCreationComand);

            var tokensAtArbitraryPaths = chamberlain.GetArbitraryPathTokenCreationCommandsToEnactLegacy(startingLegacy);
            rootCommand.TokensAtArbitraryPaths.AddRange(tokensAtArbitraryPaths);

            rootCommand.DealersTable=DealersTableForLegacy(startingLegacy);

            return rootCommand;
        }



        private static PopulateDominionCommand DealersTableForLegacy(Legacy startingLegacy)
        {
           var dealersTableCommand = new PopulateDominionCommand();

            var allDeckSpecs = Watchman.Get<Compendium>().GetEntitiesAsAlphabetisedList<DeckSpec>();
            foreach (var deckSpec in allDeckSpecs)
            {
                if (string.IsNullOrEmpty(deckSpec.ForLegacyFamily) || startingLegacy.Family == deckSpec.ForLegacyFamily)
                {
                    var drawSphereSpec = new SphereSpec(typeof(DrawPile), $"{deckSpec.Id}_draw");
                    drawSphereSpec.ActionId = deckSpec.Id;
                    var drawSphereCommand = new SphereCreationCommand(drawSphereSpec);
                    
                    
                    dealersTableCommand.Spheres.Add(drawSphereCommand);

                    var discardSphereSpec = new SphereSpec(typeof(ForbiddenPile), $"{deckSpec.Id}_forbidden");
                    discardSphereSpec.ActionId = deckSpec.Id;
                    var discardSphereCommand = new SphereCreationCommand(discardSphereSpec);
                    dealersTableCommand.Spheres.Add(discardSphereCommand);
                }
            }

            return dealersTableCommand;
        }
    }
}
